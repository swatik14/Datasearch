# DataSearch

Simple search platform for phone/owner records (Karnataka & Andhra Pradesh).

## Run

```
cd DataSearch
dotnet run --urls "http://localhost:5000"
```

Default admin login: `admin` / `admin123`

## Features
- Login / register with subscription plans (Monthly/Yearly/Agency).
- Search by State + any combination of Phone, Name, Father Name, Address, Aadhaar ID, Alternative Number, Circle.
- Subscription expiry enforced.

## Import data (20M rows)

Build importer:
```
cd Importer
dotnet run -- <path-to-app.db> <path-to-data.csv>
```

CSV columns (header optional):
`State,PhoneNumber,OwnerName,FatherName,Address,AadhaarId,AlternativeNumber,Circle`

## Notes
- SQLite database file: `app.db`.
- For production use a managed DB and disable dev cert.