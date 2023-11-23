## DPI Bypass - GoodbyeDPI explanation

***SecureDNSClient --> DPI Bypass --> Basic***

```
Mode Light: -p -r -s -m -e {sslFragment} -w --native-frag --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort}
Mode Medium: -p -r -s -m -e {sslFragment} -w --auto-ttl {AutoTTL} --min-ttl {MinTTL} --native-frag --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort}
Mode High: -p -r -s -m -e {sslFragment} -w --auto-ttl {AutoTTL} --min-ttl {MinTTL} --native-frag --wrong-seq --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort}
Mode Extreme: -p -r -s -m -f 2 -e {sslFragment} -w --auto-ttl {AutoTTL} --min-ttl {MinTTL} --native-frag --wrong-chksum --wrong-seq --max-payload --dns-addr {fallbackDNS} --dns-port {fallbackDNSPort}
Mode 1: -p -r -s -f 2 -k 2 -n -e 2
Mode 2: -p -r -s -f 2 -k 2 -n -e 40
Mode 3: -p -r -s -e 40
Mode 4: -p -r -s
Mode 5: -f 2 -e 2 --auto-ttl --reverse-frag --max-payload
Mode 6: -f 2 -e 2 --wrong-seq --reverse-frag --max-payload
```

***GoodbyeDPI arguments***
```
 -p          block passive DPI
 -r          replace Host with hoSt
 -s          remove space between host header and its value
 -m          mix Host header case (test.com -> tEsT.cOm)
 -f <value>  set HTTP fragmentation to value
 -k <value>  enable HTTP persistent (keep-alive) fragmentation and set it to value
 -n          do not wait for first segment ACK when -k is enabled
 -e <value>  set HTTPS fragmentation to value
 -a          additional space between Method and Request-URI (enables -s, may break sites)
 -w          try to find and parse HTTP traffic on all processed ports (not only on port 80)
 --port        <value>    additional TCP port to perform fragmentation on (and HTTP tricks with -w)
 --ip-id       <value>    handle additional IP ID (decimal, drop redirects and TCP RSTs with this ID).
                          This option can be supplied multiple times.
 --dns-addr    <value>    redirect UDP DNS requests to the supplied IP address (experimental)
 --dns-port    <value>    redirect UDP DNS requests to the supplied port (53 by default)
 --dnsv6-addr  <value>    redirect UDPv6 DNS requests to the supplied IPv6 address (experimental)
 --dnsv6-port  <value>    redirect UDPv6 DNS requests to the supplied port (53 by default)
 --dns-verb               print verbose DNS redirection messages
 --blacklist   <txtfile>  perform circumvention tricks only to host names and subdomains from
                          supplied text file (HTTP Host/TLS SNI).
                          This option can be supplied multiple times.
 --allow-no-sni           perform circumvention if TLS SNI can't be detected with --blacklist enabled.
 --set-ttl     <value>    activate Fake Request Mode and send it with supplied TTL value.
                          DANGEROUS! May break websites in unexpected ways. Use with care (or --blacklist).
 --auto-ttl    [a1-a2-m]  activate Fake Request Mode, automatically detect TTL and decrease
                          it based on a distance. If the distance is shorter than a2, TTL is decreased
                          by a2. If it's longer, (a1; a2) scale is used with the distance as a weight.
                          If the resulting TTL is more than m(ax), set it to m.
                          Default (if set): --auto-ttl 1-4-10. Also sets --min-ttl 3.
                          DANGEROUS! May break websites in unexpected ways. Use with care (or --blacklist).
 --min-ttl     <value>    minimum TTL distance (128/64 - TTL) for which to send Fake Request
                          in --set-ttl and --auto-ttl modes.
 --wrong-chksum           activate Fake Request Mode and send it with incorrect TCP checksum.
                          May not work in a VM or with some routers, but is safer than set-ttl.
 --wrong-seq              activate Fake Request Mode and send it with TCP SEQ/ACK in the past.
 --native-frag            fragment (split) the packets by sending them in smaller packets, without
                          shrinking the Window Size. Works faster (does not slow down the connection)
                          and better.
 --reverse-frag           fragment (split) the packets just as --native-frag, but send them in the
                          reversed order. Works with the websites which could not handle segmented
                          HTTPS TLS ClientHello (because they receive the TCP flow "combined").
 --max-payload [value]    packets with TCP payload data more than [value] won't be processed.
                          Use this option to reduce CPU usage by skipping huge amount of data
                          (like file transfers) in already established sessions.
                          May skip some huge HTTP requests from being processed.
                          Default (if set): --max-payload 1200.
```

[GoodbyeDPI full documentation](https://github.com/ValdikSS/GoodbyeDPI/blob/master/README.md)
