using System.IO;

namespace OsuServerLoader.Tools
{
    internal class FileEdit
    {
        public void ChangeAccountForOsu(string path, string nickname, string password, string CEServer)
        {
            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 140)
                {
                    lines[i] = "CredentialEndpoint = " + CEServer;
                }
                if (i == 141)
                {
                    lines[i] = "Username = " + nickname;
                }
                if (i == 148)
                {
                    lines[i] = "Password = " + password;
                }
            }

            File.WriteAllLines(path, lines);
        }
    }
}