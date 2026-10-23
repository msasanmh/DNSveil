# راهنمای کامل استفاده از DNSveil روی Linux

این سند **قدم‌به‌قدم از صفر** توضیح می‌دهد چطور DNSveil را روی Linux کلون، بیلد، نصب و اجرا کنید.

> **توجه:** رابط گرافیکی فعلی (WinForms / WPF) فقط روی Windows کار می‌کند.
> روی Linux از **CLI** و در صورت نیاز **systemd** استفاده می‌شود.

- ریپازیتوری: https://github.com/seyed-ali-002/DNSveil-Linux
- برنچ توسعه Linux: `linux`
- Upstream اصلی (Windows): https://github.com/msasanmh/DNSveil

---

## فهرست مطالب

1. [پیش‌نیازها](#1-پیشنیازها)
2. [کلون کردن پروژه](#2-کلون-کردن-پروژه)
3. [نصب .NET 6 SDK](#3-نصب-net-6-sdk)
4. [بیلد پروژه](#4-بیلد-پروژه)
5. [اجرای CLI بدون نصب سیستم‌واید](#5-اجرای-cli-بدون-نصب-سیستمواید)
6. [دستورات CLI](#6-دستورات-cli)
7. [مدیریت DNS سیستم](#7-مدیریت-dns-سیستم)
8. [نصب سیستم‌واید و systemd](#8-نصب-سیستمواید-و-systemd)
9. [Publish (باینری مستقل)](#9-publish-باینری-مستقل)
10. [سرور DNS / Proxy (SDCAgnosticServer)](#10-سرور-dns--proxy-sdcagnosticserver)
11. [مسیرهای پیکربندی و لاگ](#11-مسیرهای-پیکربندی-و-لاگ)
12. [DPI و محدودیت‌های Linux](#12-dpi-و-محدودیتهای-linux)
13. [عیب‌یابی](#13-عیبیابی)
14. [معماری فنی (برای توسعه‌دهندگان)](#14-معماری-فنی-برای-توسعهدهندگان)
15. [سوالات متداول](#15-سوالات-متداول)

---

## 1. پیش‌نیازها

### سیستم‌عامل

- توزیع مبتنی بر Linux با systemd (Ubuntu، Debian، Fedora، Arch و مشابه)
- دسترسی `sudo` برای تغییر DNS سیستم و نصب سرویس

### بسته‌های پیشنهادی

```bash
# Debian / Ubuntu
sudo apt update
sudo apt install -y git curl ca-certificates

# اختیاری ولی مفید
sudo apt install -y systemd resolvconf network-manager nftables
```

```bash
# Fedora
sudo dnf install -y git curl ca-certificates
```

```bash
# Arch
sudo pacman -S git curl ca-certificates
```

### سخت‌افزار

- معماری `x64` (linux-x64) — رایج‌ترین حالت
- برای ARM64 در صورت نیاز بعداً با `-r linux-arm64` پابلیش کنید

---

## 2. کلون کردن پروژه

### با HTTPS

```bash
git clone https://github.com/seyed-ali-002/DNSveil-Linux.git
cd DNSveil-Linux
git checkout linux
```

### با SSH (اگر کلید GitHub دارید)

```bash
git clone git@github.com:seyed-ali-002/DNSveil-Linux.git
cd DNSveil-Linux
git checkout linux
```

بررسی وضعیت:

```bash
git status
git branch -vv
# باید روی branch linux باشید
```

---

## 3. نصب .NET 6 SDK

DNSveil روی این برنچ با **.NET 6** بیلد می‌شود.

### روش رسمی (بدون وابستگی به پکیج‌منیجر)

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0
```

سپس PATH را برای همین نشست تنظیم کنید:

```bash
export PATH="$HOME/.dotnet:$PATH"
```

برای دائمی کردن (bash):

```bash
echo 'export PATH="$HOME/.dotnet:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

بررسی نصب:

```bash
dotnet --version
# انتظار: چیزی شبیه 6.0.x
```

### غیرفعال کردن تله‌متری (اختیاری)

```bash
export DOTNET_CLI_TELEMETRY_OPTOUT=1
```

---

## 4. بیلد پروژه

از ریشهٔ ریپو:

```bash
cd ~/DNSveil-Linux   # یا مسیری که کلون کرده‌اید
export PATH="$HOME/.dotnet:$PATH"
```

### قدم ۱ — بیلد لایه Platform و CLI

```bash
dotnet build DNSveil.Platform/DNSveil.Platform.csproj -c Release
dotnet build DNSveil.Platform.Linux/DNSveil.Platform.Linux.csproj -c Release
dotnet build DNSveil.Cli/DNSveil.Cli.csproj -c Release
```

اگر همه چیز درست باشد باید پیام `Build succeeded` ببینید.

### قدم ۲ — (اختیاری) بیلد سرور DNS/Proxy

```bash
dotnet build SDCAgnosticServer/SDCAgnosticServer.csproj -c Release
```

> پروژه‌های GUI (`SecureDNSClient`, `DNSveil`) مخصوص Windows هستند و روی Linux بیلد نمی‌شوند؛ این عمدی است.

### محل خروجی CLI

```text
DNSveil.Cli/bin/Release/net6.0/dnsveil.dll
DNSveil.Cli/bin/Release/net6.0/dnsveil      # apphost در صورت تولید
```

---

## 5. اجرای CLI بدون نصب سیستم‌واید

برای تست سریع، بدون `sudo install`:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd ~/DNSveil-Linux

dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll help
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll status
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll dns list
```

نمونه خروجی `status`:

```text
Platform : Linux
Config   : /home/USER/.config/dnsveil
Data     : /home/USER/.local/share/dnsveil
Log      : /home/USER/.local/state/dnsveil/log
DNS backend: SystemdResolved
Privileged : False
DNS servers: ...
Service  : Stopped
```

برای راحتی می‌توانید alias بگذارید:

```bash
alias dnsveil='dotnet ~/DNSveil-Linux/DNSveil.Cli/bin/Release/net6.0/dnsveil.dll'
dnsveil status
```

---

## 6. دستورات CLI

| دستور | توضیح |
|--------|--------|
| `dnsveil help` | راهنما |
| `dnsveil version` | نسخه |
| `dnsveil status` | وضعیت پلتفرم، DNS، سرویس |
| `dnsveil dns list` | لیست DNS فعلی سیستم |
| `dnsveil dns set <ip> [ip...]` | تنظیم DNS سیستم (**نیاز به root**) |
| `dnsveil dns restore` | بازگردانی DNS قبلی (**نیاز به root**) |
| `dnsveil dns scan` | فعلاً placeholder |
| `dnsveil config show` | مسیرها و فایل پیکربندی |
| `dnsveil config set <key> <value>` | ذخیره کلید در `dnsveil.json` |
| `dnsveil proxy status` | وضعیت پروکسی (اطلاعاتی) |
| `dnsveil start` | `systemctl start dnsveil` |
| `dnsveil stop` | `systemctl stop dnsveil` |
| `dnsveil restart` | `systemctl restart dnsveil` |

مثال‌ها:

```bash
dnsveil status
dnsveil dns list
sudo dnsveil dns set 1.1.1.1 8.8.8.8
sudo dnsveil dns restore
dnsveil config show
dnsveil config set upstream "https://cloudflare-dns.com/dns-query"
```

---

## 7. مدیریت DNS سیستم

CLI به‌صورت خودکار تشخیص می‌دهد سیستم از کدام backend استفاده می‌کند:

1. **systemd-resolved** (`resolvectl`) — ترجیح اول
2. **NetworkManager** (`nmcli`)
3. **/etc/resolv.conf** — fallback

### جریان پیشنهادی برای استفاده محلی

1. یک resolver محلی روی `127.0.0.1` اجرا کنید (مثلاً SDCAgnosticServer).
2. DNS سیستم را به آن اشاره دهید:

```bash
sudo dnsveil dns set 127.0.0.1
```

3. بعد از اتمام کار:

```bash
sudo dnsveil dns restore
```

### بررسی دستی

```bash
resolvectl status          # اگر systemd-resolved فعال است
cat /etc/resolv.conf
nmcli device show          # اگر NetworkManager فعال است
```

---

## 8. نصب سیستم‌واید و systemd

### قدم ۱ — بیلد Release

```bash
export PATH="$HOME/.dotnet:$PATH"
cd ~/DNSveil-Linux
dotnet build DNSveil.Cli -c Release
# اختیاری:
dotnet build SDCAgnosticServer -c Release
```

### قدم ۲ — اجرای اسکریپت نصب

```bash
sudo ./linux/scripts/install.sh
```

این اسکریپت:

- پوشه‌های `/etc/dnsveil`، `/var/lib/dnsveil`، `/var/log/dnsveil` را می‌سازد
- باینری CLI را در `/usr/local/bin/dnsveil` کپی می‌کند (در صورت وجود)
- در صورت وجود، `SDCAgnosticServer` را به‌عنوان `dnsveil-server` نصب می‌کند
- فایل `linux/systemd/dnsveil.service` را در `/etc/systemd/system/` قرار می‌دهد
- `systemctl daemon-reload` را اجرا می‌کند

### قدم ۳ — فعال‌سازی سرویس

فقط وقتی باینری سرور (`dnsveil-server`) نصب شده باشد معنا دارد:

```bash
sudo systemctl enable dnsveil
sudo systemctl start dnsveil
sudo systemctl status dnsveil
```

دستورات رایج:

```bash
sudo systemctl stop dnsveil
sudo systemctl restart dnsveil
journalctl -u dnsveil -f          # لاگ زنده
```

یا از طریق CLI:

```bash
sudo dnsveil start
sudo dnsveil stop
sudo dnsveil restart
dnsveil status
```

### محتویات تقریبی unit

فایل: `linux/systemd/dnsveil.service`

- بعد از `network-online.target` بالا می‌آید
- `Restart=on-failure`
- لاگ به journald
- مسیرهای نوشتنی: `/etc/dnsveil`، `/var/lib/dnsveil`، `/var/log/dnsveil`، `/etc/resolv.conf`

---

## 9. Publish (باینری مستقل)

اگر نمی‌خواهید روی ماشین مقصد SDK نصب باشد:

```bash
export PATH="$HOME/.dotnet:$PATH"
cd ~/DNSveil-Linux

mkdir -p publish/cli publish/server

dotnet publish DNSveil.Cli -c Release -r linux-x64 --self-contained true -o ./publish/cli
dotnet publish SDCAgnosticServer -c Release -r linux-x64 --self-contained true -o ./publish/server
```

اجرا:

```bash
./publish/cli/dnsveil status
./publish/server/SDCAgnosticServer
```

نسخه وابسته به runtime (کوچک‌تر؛ نیاز به `dotnet` runtime روی سیستم):

```bash
dotnet publish DNSveil.Cli -c Release -r linux-x64 --self-contained false -o ./publish/cli-fx
```

---

## 10. سرور DNS / Proxy (SDCAgnosticServer)

`SDCAgnosticServer` هستهٔ چندپروتکلی است (Plain DNS، DoH، HTTP/HTTPS Proxy، SOCKS4/4A/5) و روی `net6.0` است؛ روی Linux هم بیلد می‌شود.

```bash
dotnet build SDCAgnosticServer -c Release
dotnet SDCAgnosticServer/bin/Release/net6.0/SDCAgnosticServer.dll
```

پس از بالا آمدن سرور محلی (مثلاً روی `127.0.0.1:53` یا پورت تنظیم‌شده):

```bash
sudo dnsveil dns set 127.0.0.1
```

برای پروکسی، مرورگر یا سیستم را روی HTTP/SOCKS همان ماشین تنظیم کنید (جزئیات پورت به تنظیمات سرور بستگی دارد).

---

## 11. مسیرهای پیکربندی و لاگ

| مسیر | نقش |
|------|-----|
| `/etc/dnsveil/` | پیکربندی سیستم‌واید |
| `/var/lib/dnsveil/` | داده و state |
| `/var/log/dnsveil/` | لاگ فایل |
| `~/.config/dnsveil/` | fallback کاربر (بدون root) |
| `~/.local/share/dnsveil/` | data کاربر |
| `~/.local/state/dnsveil/log` | لاگ کاربر |

فایل نمونهٔ تنظیمات CLI:

```text
~/.config/dnsveil/dnsveil.json
# یا
/etc/dnsveil/dnsveil.json
```

مشاهده:

```bash
dnsveil config show
```

---

## 12. DPI و محدودیت‌های Linux

روی Windows از **GoodbyeDPI + WinDivert** استفاده می‌شود. معادل یک‌به‌یک روی Linux وجود ندارد.

وضعیت فعلی برنچ `linux`:

| روش | وضعیت |
|-----|--------|
| Proxy userspace (SDCAgnosticServer) | مسیر پیشنهادی و قابل استفاده |
| nftables + NFQUEUE (`LinuxDpiBypass`) | scaffold؛ برای تکمیل handler کاربران |
| GUI | هنوز Windows-only |

نصب ابزار nftables (برای مسیر DPI سطح کرنل):

```bash
# Debian/Ubuntu
sudo apt install -y nftables
```

---

## 13. عیب‌یابی

### `dotnet: command not found`

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet --version
```

### بیلد GUI شکست می‌خورد

عادی است. فقط این پروژه‌ها را روی Linux بیلد کنید:

- `DNSveil.Platform`
- `DNSveil.Platform.Linux`
- `DNSveil.Cli`
- `SDCAgnosticServer` (اختیاری)
- `MsmhToolsClass` (در صورت نیاز)

### `dnsveil dns set` خطا می‌دهد

- با `sudo` اجرا کنید
- بررسی کنید backend چیست: `dnsveil status`
- برای systemd-resolved: `resolvectl status`
- برای NetworkManager: اتصال فعال داشته باشید

### سرویس start نمی‌شود

```bash
sudo systemctl status dnsveil
journalctl -u dnsveil -n 50 --no-pager
ls -la /usr/local/bin/dnsveil-server   # باید وجود داشته باشد
```

### تغییر DNS اثر ندارد

```bash
resolvectl flush-caches
# یا
sudo systemd-resolve --flush-caches   # نسخه‌های قدیمی‌تر
```

مرورگر را یک‌بار کامل ببندید و باز کنید.

---

## 14. معماری فنی (برای توسعه‌دهندگان)

```text
DNSveil.Platform              # اینترفیس‌های مشترک
  └── Abstractions/
        ISystemDnsManager
        IServiceManager
        IPacketInterceptor
        IDpiBypass
        INetworkConfigurator
  └── PlatformFactory.cs

DNSveil.Platform.Linux        # پیاده‌سازی Linux
  ├── Dns/LinuxSystemDnsManager.cs
  ├── Service/SystemdServiceManager.cs
  ├── Network/LinuxNetworkConfigurator.cs
  ├── Dpi/LinuxDpiBypass.cs
  ├── Dpi/LinuxPacketInterceptor.cs
  └── LinuxBootstrap.cs

DNSveil.Cli                   # برنامه خط فرمان (AssemblyName: dnsveil)
linux/systemd/dnsveil.service
linux/scripts/install.sh
```

ثبت سرویس‌ها در شروع برنامه:

```csharp
if (OperatingSystem.IsLinux())
    DNSveil.Platform.Linux.LinuxBootstrap.Initialize();

var dns = PlatformFactory.CreateDnsManager();
var svc = PlatformFactory.CreateServiceManager();
```

کد Windows (WinDivert، Registry، WinForms، WPF) دست نخورده باقی مانده است.

---

## 15. سوالات متداول

**آیا GUI روی Linux دارم؟**  
خیر. فعلاً فقط CLI و زیرساخت سرویس. GUI در اولویت بعدی است.

**آیا IP من عوض می‌شود؟**  
خیر. DNSveil VPN نیست؛ DNS امن و در صورت پیکربندی، پروکسی/بای‌پس SNI است.

**حداقل دستورات برای شروع سریع چیست؟**

```bash
git clone https://github.com/seyed-ali-002/DNSveil-Linux.git
cd DNSveil-Linux && git checkout linux
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 6.0
export PATH="$HOME/.dotnet:$PATH"
dotnet build DNSveil.Cli -c Release
dotnet DNSveil.Cli/bin/Release/net6.0/dnsveil.dll status
```

**چطور به آخرین تغییرات برنچ linux برسم؟**

```bash
git fetch origin
git checkout linux
git pull origin linux
```

---

## مجوز

همان مجوز پروژه اصلی DNSveil (فایل `LICENSE` در ریشهٔ ریپو).

---

*این راهنما مربوط به برنچ `linux` در ریپازیتوری DNSveil-Linux است.*
