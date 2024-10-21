# DNSveil (formerly Secure DNS Client)

![GitHub all releases](https://img.shields.io/github/downloads/msasanmh/SecureDNSClient/total)
![GitHub top language](https://img.shields.io/github/languages/top/msasanmh/SecureDNSClient)
![GitHub](https://img.shields.io/github/license/msasanmh/SecureDNSClient)
![GitHub release (with filter)](https://img.shields.io/github/v/release/msasanmh/SecureDNSClient?link=https%3A%2F%2Fgithub.com%2Fmsasanmh%2FSecureDNSClient%2Freleases%2Flatest)

**A Secure DNS Client.** Using: _[Msmh Agnostic Server](https://github.com/msasanmh/MsmhAgnosticServer)_, _[DNSLookup](https://github.com/ameshkov/dnslookup)_ and _[GoodbyeDPI](https://github.com/ValdikSS/GoodbyeDPI)_. (Windows only)

Client implementation: _DNSCrypt, Anonymized DNSCrypt, DoH, DoT and Plain DNS (UDP & TCP)._<br>
Server implementation: _DoH and Plain DNS (UDP & TCP)._

- *Find and use fastest secure DNS servers.*
- *Hide SNI and website addresses from ISP (Fragment or Fake SNI).*
- *Bypass YouTube, Twitter and any SNI/DNS based blocked websites.*
- *Encode and decode DNSCrypt STAMP (sdns://).*
- *Share to other devices via Proxy (HTTP, HTTPS, SOCKS4, SOCKS4A, SOCKS5).*

**Requirements:** `.Net Destop Runtime 6` and `ASP.NET Core Runtime 6`

For x64:\
First install [.Net Destop Runtime x64 v6.0.35](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.35-windows-x64-installer)\
Then install [ASP.NET Core Runtime x64 v6.0.35](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-6.0.35-windows-x64-installer)

For x86:\
First install [.Net Destop Runtime x86 v6.0.35](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-6.0.35-windows-x86-installer)\
Then install [ASP.NET Core Runtime x86 v6.0.35](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-6.0.35-windows-x86-installer)

[Microsoft .NET 6.0 Runtime Page](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

**Download:**\
[Download latest version of DNSveil](https://github.com/msasanmh/SecureDNSClient/releases/latest)

---

### Notes
* DNSveil is not a VPN and it does not change your IP address, so your IP address is still visible to the websites you visit.
* DNSveil is open source and super clean. If your Antivirus raise an alert it's False-Positive, any programmer can read the source and confirm it. some antivirus apps raise alert as PUA (Potentially Unwanted Application) for WinDivert which is used by GoodbyeDPI. If your antivirus detects WinDivert as a threat, add it to your exclusion list to ensure DNSveil functions as expected.
* After changing `Enable SSL Decryption` you need to restart your browser in order the changes to take effect.

---

### Features
* Connect with built-in servers or use your own custom servers.
* Find fastest DNS Servers.
* Bypass any SNI/DNS based blocked websites by Fragment and Fake SNI.
* Create local Plain DNS and DoH Servers.
* Supports per domain rules.
* Advanced DNS Scanner.
    - Detection of Google safe search.
    - Detection of Bing safe search.
    - Detection of blocked or restricted Youtube.
    - Detection of blocked Adult Content.
    - Export online servers based on condition.
* DNS Lookup.
* Cloudflare clean IP scanner.
* Can Read/Modify/Generate STAMP (sdns://) URLs.
* Run and connect on Windows Startup.
* Import servers from text files.
* Extract and import servers from URLs.
* Double-Click on a custom server to get info and status.
* Import/Export all settings.

---

### Supported Protocols
* **DNSCrypt**
    - Must be in STAMP format. e.g.
        - `sdns://AQcAAAAAAAAAETg5LjM4LjEzMS4zODo0MzQzIKWHS9r0FoKY--wcnJl1Ar5aOUb91xsufvPUjid3rNRaHzIuZG5zY3J5cHQtY2VydC5hbXMtZG5zY3J5cHQtbmw`
* **Anonymized DNSCrypt**
    - Pattern: `<DNSCrypt Server in STAMP format>` `<Space>` `<DNSCrypt Relay>`
    - `<DNSCrypt Relay>` can be in STAMP or IP:PORT format.
    - Example:
        - `sdns://AQcAAAAAAAAAETg5LjM4LjEzMS4zODo0MzQzIKWHS9r0FoKY--wcnJl1Ar5aOUb91xsufvPUjid3rNRaHzIuZG5zY3J5cHQtY2VydC5hbXMtZG5zY3J5cHQtbmw sdns://gQ4xNzcuNTQuMTQ1LjEzMQ`
        - `sdns://AQcAAAAAAAAAETg5LjM4LjEzMS4zODo0MzQzIKWHS9r0FoKY--wcnJl1Ar5aOUb91xsufvPUjid3rNRaHzIuZG5zY3J5cHQtY2VydC5hbXMtZG5zY3J5cHQtbmw 177.54.145.131:443`
* **DoH (DNS Over HTTPS)**
    - Example (HTTP/2):
        - `https://max.rethinkdns.com/dns-query`
    - Example (HTTP/3):
        - `h3://max.rethinkdns.com/dns-query`
* **DoT (DNS Over TLS)**
    - Example:
        - `tls://dns.quad9.net`
* **Plain DNS (UDP & TCP)**
    - Example:
        - `udp://8.8.8.8:53`
        - `tcp://1.1.1.1:53`

---

### Proxy Server
* Proxy server is use to bypass SNI/DNS based blocked websites.
* How to use:
    1. DNSveil DNS Server must be online and set to System.
    2. Atleast one of DPI Bypass options must be active.
        - Fragment
        - SSL Decryption (by installing self-signed root certificate authority)
            - Enable `Change SNI` and provide a fake SNI.
    3. Start Proxy Server.
    4. Set Proxy to System.

---

### DNSveil Text Based Rules
### SDC Text Based Rules
* Syntax (wildcard is supported):
    - `Domain` `|` `Rules` `;`
    - `CIDR` `|` `Rules` `;`
    - `IPv4` `|` `Rules` `;`
    - `IPv6` `|` `Rules` `;`
* Rules:
    - Fake DNS (forward a domain to your desired IP address):\
    `example.com|127.0.0.1;`
    - Use a custom DNS for a domain:\
    `example.com|dns:https://max.rethinkdns.com/dns-query;`
    - Use a custom and blocked DNS by an upstream proxy:\
    `example.com|dns:https://max.rethinkdns.com/dns-query;dnsproxy:socks5://127.0.0.1:1080;`
    - DNS Domain (Get IP for a domain and use it for another domain):\
    `youtube.com|dnsdomain:google.com;`
    - Use upstream proxy for a domain (only socks5 and http are supported):\
    `example.com|proxy:socks5://127.0.0.1:1080;`\
    `example.com|proxy:http://127.0.0.1:1080;`
    - Use upstream proxy with user and pass:\
    `example.com|proxy:socks5://127.0.0.1:1080&user:UserName&pass:PassWord;`
    - Set a custom/fake SNI for a domain:\
    `*.googlevideo.com|sni:google.com;`
    - Direct: Don't apply DPI bypass and upstream proxy for a domain:\
    `example.com|--;`\
    `*.example.com|--;`
    - Block a domain and all it's sub-domains:\
    `example.com|-;`\
    `*.example.com|-;`
    - Block CIDR (IP Range):\
    `224.0.0.0/3|-;`\
    `fe80::/10|-;`
<br><br>
* Example of Rules file:
```
// Variables
SmartDns1 = https://one.YourSmartDnsServer.net/dns-query;
SmartDns2 = https://two.YourSmartDnsServer.net/dns-query;
SmartDns3 = https://three.YourSmartDnsServer.net/dns-query;

// Defaults
blockport:53,80;

// YouTube
youtube.com|dnsdomain:google.com;sni:google.com;
ytimg.com|dnsdomain:google.com;
*.ytimg.com|dnsdomain:google.com;
ggpht.com|dnsdomain:google.com;
*.ggpht.com|dnsdomain:*.googleusercontent.com;
*.googleapis|dnsdomain:google.com;
*.googlevideo.com|dnsdomain:*.c.docs.google.com;sni:google.com;

// Use Smart DNS For These Domains
developers.google.com|--;dns:SmartDns1,SmartDns2,SmartDns3;
*.googleusercontent.com|--;dns:SmartDns1,SmartDns2,SmartDns3;
developer.android.com|--;dns:SmartDns1,SmartDns2,SmartDns3;
gemini.google.com|--;dns:SmartDns1,SmartDns2,SmartDns3;
*.openai.com|--;dns:SmartDns1;
claude.ai|--;dns:SmartDns1,SmartDns2,SmartDns3;
*.claude.ai|--;dns:SmartDns1,SmartDns2,SmartDns3;
spotify.com|--;dns:SmartDns1,SmartDns2,SmartDns3;
*.spotify.com|--;dns:SmartDns1,SmartDns2,SmartDns3;

// Don't Apply DPI Bypass To These Domains
google.com|--;
*.google.com|--;
github.com|--;
*.github.com|--;
githubusercontent.com|--;
*.githubusercontent.com|--;
stackoverflow.com|--;
*.stackoverflow.com|--;
*.sstatic.net|--;
*.cookielaw.org|--;
every1dns.com|--;
*.every1dns.com|--;
nslookup.io|--;
*.nslookup.io|--;
php.net|--;
save.tube|--;

// Apply Defaults To Other Domains
*|+;
```

---

[Find Videos in Help Directory.](https://github.com/msasanmh/SecureDNSClient/tree/main/Help)\
[Another Guide.](https://rentry.co/SecureDNSClient)


