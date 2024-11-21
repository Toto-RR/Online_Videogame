using System.Net;
using UnityEngine;
using TMPro;

public class GetIP : MonoBehaviour
{
    private TextMeshProUGUI m_TextMeshPro;

    private void Start()
    {
        m_TextMeshPro = GetComponent<TextMeshProUGUI>();
        if (m_TextMeshPro != null)
        {
            m_TextMeshPro.text = "IP: " + GetLocalIPAddress();
        }
    }

    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }
}
