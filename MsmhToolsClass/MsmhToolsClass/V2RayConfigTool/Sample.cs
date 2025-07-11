using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsmhToolsClass.V2RayConfigTool;

public class Sample
{
    public static string ClientConfig()
    {
        string sample = "{\r\n\t\"log\": {\r\n\t\t\"access\": \"Vaccess.log\",\r\n\t\t\"error\": \"Verror.log\",\r\n\t\t\"loglevel\": \"warning\"\r\n\t},\r\n\t\"inbounds\": [],\r\n\t\"outbounds\": [\r\n\t\t{\r\n\t\t\t\"tag\": \"proxy\",\r\n\t\t\t\"protocol\": \"vmess\",\r\n\t\t\t\"settings\": {\r\n\t\t\t\t\"vnext\": [{\r\n\t\t\t\t\t\"address\": \"\",\r\n\t\t\t\t\t\"port\": 0,\r\n\t\t\t\t\t\"users\": [{\r\n\t\t\t\t\t\t\"id\": \"\",\r\n\t\t\t\t\t\t\"security\": \"auto\"\r\n\t\t\t\t\t}]\r\n\t\t\t\t}],\r\n\t\t\t\t\"servers\": [{\r\n\t\t\t\t\t\"address\": \"\",\r\n\t\t\t\t\t\"method\": \"\",\r\n\t\t\t\t\t\"ota\": false,\r\n\t\t\t\t\t\"password\": \"\",\r\n\t\t\t\t\t\"port\": 0,\r\n\t\t\t\t\t\"level\": 1\r\n\t\t\t\t}]\r\n\t\t\t},\r\n\t\t\t\"streamSettings\": {\r\n\t\t\t\t\"network\": \"tcp\"\r\n\t\t\t},\r\n\t\t\t\"mux\": {\r\n\t\t\t\t\"enabled\": false\r\n\t\t\t}\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"protocol\": \"freedom\",\r\n\t\t\t\"tag\": \"direct\"\r\n\t\t},\r\n\t\t{\r\n\t\t\t\"protocol\": \"blackhole\",\r\n\t\t\t\"tag\": \"block\"\r\n\t\t}\r\n\t],\r\n\t\"routing\": {\r\n\t\t\"domainStrategy\": \"IPIfNonMatch\",\r\n\t\t\"rules\": [\r\n\t\t\t{\r\n\t\t\t\t\"inboundTag\": [\r\n\t\t\t\t\t\"api\"\r\n\t\t\t\t],\r\n\t\t\t\t\"outboundTag\": \"api\",\r\n\t\t\t\t\"type\": \"field\"\r\n\t\t\t}\r\n\t\t]\r\n\t}\r\n}";

        

        return sample;
    }
}