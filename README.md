# Meghna MCP Server — Setup ও Testing Guideline (Ollama + ollmcp দিয়ে)

এই ডকুমেন্টে ধাপে ধাপে দেখানো হয়েছে কীভাবে **Meghna MCP Server** (ASP.NET Core / .NET 10) চালিয়ে, **Ollama** এর local model ও **ollmcp** client দিয়ে connect করে টেস্ট করা যায় — সম্পূর্ণ ফ্রি, কোনো subscription/account ছাড়াই।

---

## প্রয়োজনীয় জিনিস

- .NET 10 SDK ইনস্টল করা থাকতে হবে
- [Ollama](https://ollama.com) ইনস্টল করা থাকতে হবে
- Python + pip ইনস্টল করা থাকতে হবে (ollmcp-এর জন্য)
- একটা **tool-calling সাপোর্ট করে এমন** Ollama model (যেমন `qwen3:8b`, `qwen2.5`, `llama3.1`, `mistral-nemo`)
  - **⚠️ `nomic-embed-text` এর মতো embedding-only model দিয়ে কাজ হবে না** — এটা tool call করতে পারে না।

---

## ধাপ ১ — Oracle connection string সেট করুন (একবারই)

প্রজেক্ট ফোল্ডারে গিয়ে:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:OracleConnection" "User Id=YOUR_USER;Password=YOUR_PASSWORD;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=YOUR_HOST)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=YOUR_SERVICE)));Validate Connection=true;Connection Lifetime=60;Min Pool Size=1;Connection Timeout=15;"
```

> `appsettings.json` / `appsettings.Development.json`-এ আসল credential রাখবেন না।

---

## ধাপ ২ — সার্ভার চালু করুন

প্রজেক্ট ফোল্ডারে:

```bash
dotnet run
```

Console-এ একটা URL দেখাবে, যেমন:
```
Now listening on: https://localhost:52249
```

**এই টার্মিনাল উইন্ডোটা চালু/খোলা রাখতে হবে** — বন্ধ করলে সার্ভারও বন্ধ হয়ে যাবে এবং tools connect হবে না।

### Verify করুন সার্ভার সাড়া দিচ্ছে কিনা

আরেকটা টার্মিনালে (PowerShell হলে `curl.exe` ব্যবহার করুন, `curl` alias না):

```powershell
curl.exe -k https://localhost:52249/
```

Expected output:
```json
{"application":"Meghna MCP Server","version":"1.0","framework":".NET 10","authentication":false,"status":"Running"}
```

> নোট: IIS Express/Kestrel সাধারণত self-signed HTTPS certificate ব্যবহার করে, তাই `-k` (insecure) ফ্ল্যাগ লাগবে টেস্টের সময়।

---

## ধাপ ৩ — Ollama মডেল ready আছে কিনা দেখুন

```bash
ollama list
```

Tool-calling model (যেমন `qwen3:8b`) তালিকায় না থাকলে:

```bash
ollama pull qwen2.5
```

---

## ধাপ ৪ — ollmcp ইনস্টল করুন

```bash
pip install --upgrade ollmcp
```

---

## ধাপ ৫ — MCP সার্ভার যোগ করুন

```bash
ollmcp mcp add --transport http meghna https://localhost:52249/mcp
```

Verify করতে:
```bash
ollmcp mcp list
```

---

## ধাপ ৬ — ollmcp চালু করে টেস্ট করুন

```bash
ollmcp
```

TUI খুললে:

1. `/model` টাইপ করে tool-calling সাপোর্ট থাকা মডেল (`qwen3:8b`) সিলেক্ট করুন → নাম্বার দিয়ে সিলেক্ট → `s` দিয়ে save
2. Tools panel-এ `meghna.get_quotation` দেখা উচিত (✓ enabled)
3. প্রশ্ন করুন, যেমন:
   ```
   ADC260824000043 quotation-er basePremium koto?
   ```
4. মডেল tool call করে Oracle থেকে ডেটা এনে উত্তর দেবে।

---

## সাধারণ সমস্যা ও সমাধান (Troubleshooting)

| সমস্যা | কারণ | সমাধান |
|---|---|---|
| "No Tools Available" | dotnet সার্ভার বন্ধ, অথবা ভুল port/URL দেওয়া হয়েছে | `dotnet run` আবার চালান, port ঠিক আছে কিনা যাচাই করুন |
| Tools আছে কিন্তু মডেল উত্তর দিচ্ছে না ঠিকমতো | `nomic-embed-text` এর মতো non-tool model সিলেক্ট করা আছে | `/model` দিয়ে `qwen3:8b` সিলেক্ট করুন |
| `curl -k` কাজ করছে না PowerShell-এ | PowerShell-এ `curl` আসলে `Invoke-WebRequest` alias | `curl.exe -k ...` ব্যবহার করুন |
| "Could not connect to server" | dotnet সার্ভার আসলে বন্ধ হয়ে গেছে | সার্ভার টার্মিনাল খোলা আছে কিনা চেক করুন, না থাকলে আবার `dotnet run` |
| HTTPS certificate error | Self-signed dev certificate | টেস্ট URL-এ `-k` ফ্ল্যাগ ব্যবহার করুন (শুধু dev/local-এর জন্য) |

---

## প্রতিদিনের ব্যবহারের জন্য সংক্ষিপ্ত checklist

1. ✅ `dotnet run` (সার্ভার টার্মিনাল খোলা রাখুন)
2. ✅ নতুন টার্মিনালে `ollmcp`
3. ✅ `/model` → `qwen3:8b` সিলেক্ট
4. ✅ Tools panel-এ `meghna.get_quotation` কনফার্ম করুন
5. ✅ প্রশ্ন করুন
