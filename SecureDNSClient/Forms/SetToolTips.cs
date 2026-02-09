using MsmhToolsWinFormsClass;

namespace SecureDNSClient;

public partial class FormMain : Form
{
    private void SetToolTips()
    {
        // Add Tooltips
        string msgCheckInParallel = "Set to 1 to scan in series.";
        CustomNumericUpDownCheckInParallel.SetToolTip(MainToolTip, "Info", msgCheckInParallel);

        string msgCS = "Manage Custom Servers.";
        CustomButtonEditCustomServers.SetToolTip(MainToolTip, "Info", msgCS);

        string msgCH = "Right click to access more options.";
        CustomButtonCheck.SetToolTip(MainToolTip, "Info", msgCH);

        string msgQC = "Right click to access more options.";
        CustomButtonQuickConnect.SetToolTip(MainToolTip, "Info", msgQC);

        string msgCheckUpdate = "Check for new application version.";
        CustomButtonCheckUpdate.SetToolTip(MainToolTip, "Info", msgCheckUpdate);

        string msgUpdateAutoDelay = "More = Less CPU Usage but app's Reaction Speed will Decrease.";
        msgUpdateAutoDelay += "\nLess = More CPU Usage but app's Reaction Speed will Increase.";
        CustomNumericUpDownUpdateAutoDelayMS.SetToolTip(MainToolTip, "Info", msgUpdateAutoDelay);

        // Add Tooltips to advanced DPI
        string msgP = "Block passive DPI.";
        CustomCheckBoxDPIAdvP.SetToolTip(MainToolTip, "Info", msgP);
        string msgR = "Replace Host with hoSt.";
        CustomCheckBoxDPIAdvR.SetToolTip(MainToolTip, "Info", msgR);
        string msgS = "Remove space between host header and its value.";
        CustomCheckBoxDPIAdvS.SetToolTip(MainToolTip, "Info", msgS);
        string msgM = "Mix Host header case (test.com -> tEsT.cOm).";
        CustomCheckBoxDPIAdvM.SetToolTip(MainToolTip, "Info", msgM);
        string msgF = "Set HTTP fragmentation to value";
        CustomCheckBoxDPIAdvF.SetToolTip(MainToolTip, "Info", msgF);
        string msgK = "Enable HTTP persistent (keep-alive) fragmentation and set it to value.";
        CustomCheckBoxDPIAdvK.SetToolTip(MainToolTip, "Info", msgK);
        string msgN = "Do not wait for first segment ACK when -k is enabled.";
        CustomCheckBoxDPIAdvN.SetToolTip(MainToolTip, "Info", msgN);
        string msgE = "Set HTTPS fragmentation to value.";
        CustomCheckBoxDPIAdvE.SetToolTip(MainToolTip, "Info", msgE);
        string msgA = "Additional space between Method and Request-URI (enables -s, may break sites).";
        CustomCheckBoxDPIAdvA.SetToolTip(MainToolTip, "Info", msgA);
        string msgW = "Try to find and parse HTTP traffic on all processed ports (not only on port 80).";
        CustomCheckBoxDPIAdvW.SetToolTip(MainToolTip, "Info", msgW);
        string msgPort = "Additional TCP port to perform fragmentation on (and HTTP tricks with -w).";
        CustomCheckBoxDPIAdvPort.SetToolTip(MainToolTip, "Info", msgPort);
        string msgIpId = "Handle additional IP ID (decimal, drop redirects and TCP RSTs with this ID).";
        CustomCheckBoxDPIAdvIpId.SetToolTip(MainToolTip, "Info", msgIpId);
        string msgAllowNoSni = "Perform circumvention if TLS SNI can't be detected with --blacklist enabled.";
        CustomCheckBoxDPIAdvAllowNoSNI.SetToolTip(MainToolTip, "Info", msgAllowNoSni);
        string msgSetTtl = "Activate Fake Request Mode and send it with supplied TTL value.\nDANGEROUS! May break websites in unexpected ways. Use with care(or--blacklist).";
        CustomCheckBoxDPIAdvSetTTL.SetToolTip(MainToolTip, "Info", msgSetTtl);
        string msgAutoTtl = "Activate Fake Request Mode, automatically detect TTL and decrease\nit based on a distance. If the distance is shorter than a2, TTL is decreased\nby a2. If it's longer, (a1; a2) scale is used with the distance as a weight.\nIf the resulting TTL is more than m(ax), set it to m.\nDefault (if set): --auto-ttl 1-4-10. Also sets --min-ttl 3.\nDANGEROUS! May break websites in unexpected ways. Use with care (or --blacklist).";
        CustomCheckBoxDPIAdvAutoTTL.SetToolTip(MainToolTip, "[a1-a2-m]", msgAutoTtl);
        string msgMinTtl = "Minimum TTL distance (128/64 - TTL) for which to send Fake Request\nin --set - ttl and--auto - ttl modes.";
        CustomCheckBoxDPIAdvMinTTL.SetToolTip(MainToolTip, "Info", msgMinTtl);
        string msgWrongChksum = "Activate Fake Request Mode and send it with incorrect TCP checksum.\nMay not work in a VM or with some routers, but is safer than set - ttl.";
        CustomCheckBoxDPIAdvWrongChksum.SetToolTip(MainToolTip, "Info", msgWrongChksum);
        string msgWrongSeq = "Activate Fake Request Mode and send it with TCP SEQ/ACK in the past.";
        CustomCheckBoxDPIAdvWrongSeq.SetToolTip(MainToolTip, "Info", msgWrongSeq);
        string msgNativeFrag = "Fragment (split) the packets by sending them in smaller packets, without\nshrinking the Window Size. Works faster(does not slow down the connection)\nand better.";
        CustomCheckBoxDPIAdvNativeFrag.SetToolTip(MainToolTip, "Info", msgNativeFrag);
        string msgReverseFrag = "Fragment (split) the packets just as --native-frag, but send them in the\nreversed order. Works with the websites which could not handle segmented\nHTTPS TLS ClientHello(because they receive the TCP flow \"combined\").";
        CustomCheckBoxDPIAdvReverseFrag.SetToolTip(MainToolTip, "Info", msgReverseFrag);
        string msgMaxPayload = "Packets with TCP payload data more than [value] won't be processed.\nUse this option to reduce CPU usage by skipping huge amount of data\n(like file transfers) in already established sessions.\nMay skip some huge HTTP requests from being processed.\nDefault(if set): --max-payload 1200.";
        CustomCheckBoxDPIAdvMaxPayload.SetToolTip(MainToolTip, "Info", msgMaxPayload);
        string msgBlacklist = "Perform circumvention tricks only to host names and subdomains from\nsupplied text file(HTTP Host / TLS SNI).";
        CustomCheckBoxDPIAdvBlacklist.SetToolTip(MainToolTip, "Info", msgBlacklist);
    }
}