using Microsoft.Data.Sqlite;

if (args.Length < 2)
{
    Console.WriteLine("Usage: Importer <dbPath> <csvPath> [stateFilter]");
    Console.WriteLine("CSV columns: State,PhoneNumber,OwnerName,FatherName,Address,AadhaarId,AlternativeNumber,Circle");
    return;
}
var dbPath = args[0];
var csvPath = args[1];

using var conn = new SqliteConnection($"Data Source={dbPath}");
conn.Open();
using (var pragma = conn.CreateCommand()) { pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=OFF;"; pragma.ExecuteNonQuery(); }

using (var create = conn.CreateCommand())
{
    create.CommandText = @"CREATE TABLE IF NOT EXISTS Records (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        State TEXT NOT NULL,
        PhoneNumber TEXT,
        OwnerName TEXT,
        FatherName TEXT,
        Address TEXT,
        AadhaarId TEXT,
        AlternativeNumber TEXT,
        Circle TEXT
    );";
    create.ExecuteNonQuery();
}
using (var idx = conn.CreateCommand())
{
    idx.CommandText = @"CREATE INDEX IF NOT EXISTS IX_Records_State ON Records(State);
    CREATE INDEX IF NOT EXISTS IX_Records_PhoneNumber ON Records(PhoneNumber);
    CREATE INDEX IF NOT EXISTS IX_Records_OwnerName ON Records(OwnerName);
    CREATE INDEX IF NOT EXISTS IX_Records_FatherName ON Records(FatherName);
    CREATE INDEX IF NOT EXISTS IX_Records_AadhaarId ON Records(AadhaarId);
    CREATE INDEX IF NOT EXISTS IX_Records_AlternativeNumber ON Records(AlternativeNumber);
    CREATE INDEX IF NOT EXISTS IX_Records_Circle ON Records(Circle);";
    idx.ExecuteNonQuery();
}
var tx = conn.BeginTransaction();
using var cmd = conn.CreateCommand();
cmd.Transaction = tx;
cmd.CommandText = @"INSERT INTO Records (State,PhoneNumber,OwnerName,FatherName,Address,AadhaarId,AlternativeNumber,Circle)
VALUES ($state,$phone,$name,$father,$addr,$id,$alt,$circle);";
var pState = cmd.Parameters.Add("$state", Microsoft.Data.Sqlite.SqliteType.Text);
var pPhone = cmd.Parameters.Add("$phone", Microsoft.Data.Sqlite.SqliteType.Text);
var pName = cmd.Parameters.Add("$name", Microsoft.Data.Sqlite.SqliteType.Text);
var pFather = cmd.Parameters.Add("$father", Microsoft.Data.Sqlite.SqliteType.Text);
var pAddr = cmd.Parameters.Add("$addr", Microsoft.Data.Sqlite.SqliteType.Text);
var pId = cmd.Parameters.Add("$id", Microsoft.Data.Sqlite.SqliteType.Text);
var pAlt = cmd.Parameters.Add("$alt", Microsoft.Data.Sqlite.SqliteType.Text);
var pCircle = cmd.Parameters.Add("$circle", Microsoft.Data.Sqlite.SqliteType.Text);

long count = 0; long batch = 0;
foreach (var row in ReadCsv(csvPath))
{
    if (row.Length < 8) continue;
    // skip header
    if (count == 0 && row[0].Equals("State", StringComparison.OrdinalIgnoreCase)) { continue; }
    pState.Value = row[0];
    pPhone.Value = row[1];
    pName.Value = row[2];
    pFather.Value = row[3];
    pAddr.Value = row[4];
    pId.Value = row[5];
    pAlt.Value = row[6];
    pCircle.Value = row[7];
    cmd.ExecuteNonQuery();
    count++; batch++;
    if (batch >= 5000)
    {
        tx.Commit();
        Console.WriteLine($"Imported {count} rows...");
        tx.Dispose();
        var newTx = conn.BeginTransaction();
        tx = newTx; // can't reassign readonly? tx is using var, ok
        cmd.Transaction = tx;
        batch = 0;
    }
}
tx.Commit();
Console.WriteLine($"Done. Total imported: {count}");

static IEnumerable<string[]> ReadCsv(string path)
{
    using var sr = new StreamReader(path);
    string? line;
    while ((line = sr.ReadLine()) != null)
    {
        yield return ParseCsvLine(line);
    }
}

static string[] ParseCsvLine(string line)
{
    var fields = new List<string>();
    var sb = new System.Text.StringBuilder();
    bool inQuotes = false;
    for (int i = 0; i < line.Length; i++)
    {
        char c = line[i];
        if (inQuotes)
        {
            if (c == '"')
            {
                if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else inQuotes = false;
            }
            else sb.Append(c);
        }
        else
        {
            if (c == '"') inQuotes = true;
            else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }
    }
    fields.Add(sb.ToString());
    return fields.ToArray();
}