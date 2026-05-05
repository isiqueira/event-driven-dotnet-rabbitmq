using System.Security.Cryptography;
using System.Text;

namespace Shared.Events;

public static class DeterministicEventId
{
    public static Guid Create(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, bytes.Length);
        return new Guid(bytes);
    }
}
