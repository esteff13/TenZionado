# Task 4 - Security refactor

Tool: Google Gemini (gemini.google.com), 2026-10-02

## Prompt

```text
Act as a security engineer. Diagnose this C# method for SQL injection risks and memory/resource leaks, then refactor it using parameterized queries and using statements. Do not hardcode the connection string.

public string GetUserRegistration(string inputEmail) {
   string connStr = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";
   SqlConnection conn = new SqlConnection(connStr);
   conn.Open();
   SqlCommand cmd = new SqlCommand("SELECT * FROM Registrations WHERE Email = '" + inputEmail + "'", conn);
   return cmd.ExecuteScalar().ToString();
}
```

## Gemini output

Here is the security and code quality diagnosis, followed by the refactored implementation.

## Diagnosis

1. **High Severity — SQL Injection:**
   - **Mechanism:** The method constructs SQL by concatenating user-controlled input (`inputEmail`) directly into the command string:
     ```csharp
     "SELECT * FROM Registrations WHERE Email = '" + inputEmail + "'"
     ```
   - **Impact:** An attacker can break out of the string literal by passing payloads like `' OR '1'='1` or append stacked queries (e.g., `'; DROP TABLE Registrations; --`), allowing unauthorized data access, database modification, or administrative privilege escalation.
2. **Medium Severity — Resource/Connection Leaks:**
   - **Mechanism:** Neither `SqlConnection` nor `SqlCommand` are enclosed in `using` blocks or properly disposed of in a `finally` block.
   - **Impact:** If an unhandled exception occurs (such as a transient database failure, timeout, or invalid query syntax), unmanaged database connection handles and command resources will remain open until finalizer runs. In high-traffic scenarios, this exhausts the ADO.NET connection pool and leads to denial of service (`System.InvalidOperationException: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool`).
3. **Medium Severity — Hardcoded Sensitive Credentials:**
   - **Mechanism:** Plaintext database credentials (`User Id`, `Password`) are compiled directly into the binary.
   - **Impact:** Extractable via decompilation tools or repository exposure. Credentials must come from external configuration sources (environment variables, AWS Secrets Manager, Azure Key Vault, or `IConfiguration`).
4. **Low Severity / Edge Case — NullReferenceException:**
   - **Mechanism:** `cmd.ExecuteScalar().ToString()` assumes a record always matches and the returned column is not `NULL`.
   - **Impact:** If no match is found, `ExecuteScalar()` returns `null`, causing an unhandled `NullReferenceException`. Additionally, `SELECT *` combined with `ExecuteScalar()` is ambiguous; specify the target column explicitly (e.g., `SELECT RegistrationId`).

## Refactored C# Implementation

This refactoring resolves all diagnosed issues:

- Uses `using` declarations for deterministic resource disposal.
- Implements a parameterized query (`@Email`) with an explicit SQL type and size.
- Reads the connection string via dependency injection (`IConfiguration`) rather than hardcoding it.
- Handles potential `null` returns from `ExecuteScalar()`.

```csharp
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class RegistrationService
{
    private readonly string _connectionString;

    public RegistrationService(IConfiguration configuration)
    {
        // Retrieve connection string securely from appsettings.json, environment variables, or Secret Manager
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
    }

    public string? GetUserRegistration(string inputEmail)
    {
        if (string.IsNullOrWhiteSpace(inputEmail))
        {
            return null;
        }

        // Query only the required single column instead of SELECT *
        const string sql = "SELECT RegistrationDetails FROM Registrations WHERE Email = @Email";

        // 'using' statements ensure reliable disposal even on exception
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);

        // Explicitly typed parameters eliminate SQL injection and protect query cache plans
        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 256)
        {
            Value = inputEmail
        });

        conn.Open();
        object? result = cmd.ExecuteScalar();

        // Safely evaluate potential null or DBNull returns
        return (result == null || result == DBNull.Value) ? null : result.ToString();
    }
}
```

Want to convert this method to an async implementation (`ExecuteScalarAsync`)?
