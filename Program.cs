using System;
using System.IO;
using System.Text;
using Renci.SshNet;

namespace extract
{
	class MainClass
	{
		public static string Between(string data, string FirstString, string LastString)
		{
			string STR = data;
			string FinalString = "";

			int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
			int Pos2 = STR.IndexOf(LastString);
			try
			{
				FinalString = STR.Substring(Pos1, Pos2 - Pos1);
			}
			catch(Exception error)
			{
				Console.WriteLine ("\nCouldn't find the data");
				FinalString = "";
			}
			return FinalString;
		
		}

		public static String getPath (String username, String password, String host)
		{
			var ssh = new Renci.SshNet.SshClient (host, username, password);

			try
			{
				ssh.Connect();
			}
			catch(Exception error)
			{
				Console.WriteLine ("Couldn't connect to iPhone");
			}

			if (ssh.IsConnected) 
			{
				Console.WriteLine ("\nConnected to iPhone (" + host + ")");
			}

			else 
			{
				Console.WriteLine ("Could't connect to iPhone");
			}

			Console.WriteLine ("\nfind /var/mobile/Applications -iname WhatsApp.app");
			var cmd = ssh.CreateCommand("find /var/mobile/Applications -iname WhatsApp.app");   //  very long list
			var asynch = cmd.BeginExecute(delegate(IAsyncResult ar)
			{
					Console.WriteLine("\nDisconnected from iPhone.");
			}, null);

			var reader = new StreamReader(cmd.OutputStream);
				
			String output = "";
			while (!asynch.IsCompleted)
			{
				var result = reader.ReadToEnd();
				if (string.IsNullOrEmpty(result))
					continue;
				Console.Write("\n> "+result);
				output = result;
			}
				cmd.EndExecute(asynch);

			String path = output.Substring(0, 61);


			reader.Close ();
			ssh.Disconnect ();


			return path;
		}

		public static void getData(String WAPath, String username, String password, String host)
		{
			String localPath = "Cache.db-wal";
			String remotePath = WAPath + "/Library/Caches/net.whatsapp.WhatsApp/Cache.db-wal";
			String pwFile = WAPath + "/Library/pw.dat";

			var scp = new Renci.SshNet.ScpClient (host, username, password);

			try
			{
				scp.Connect ();
			}
			catch(Exception error) 
			{
				Console.WriteLine ("Coundn't connect to iPhone");
			}

			if (scp.IsConnected) 
			{
				Console.WriteLine ("SCP started ("+host+"). Downloading files...");
			}
				
			try
			{
				Stream cacheFile = File.OpenWrite(localPath);
				scp.Download (remotePath, cacheFile);
				cacheFile.Close();
								
			}
			catch(Exception error)
			{
				Console.WriteLine ("\nCache.db-wal not found");
			}
			try
			{
				Stream filePW = File.OpenWrite("pw.dat");
				scp.Download (pwFile, filePW);
				filePW.Close();
			}
			catch(Exception error)
			{
				Console.WriteLine ("\npw.dat not found\n");
			}

			scp.Disconnect ();
				
		}


		public static void Main (string[] args)
		{
			Console.WriteLine("###########################################");
			Console.WriteLine("#                                         #");
			Console.WriteLine("#    WA Password and Identity Extractor   #");
			Console.WriteLine("#              for iPhone                 #");
			Console.WriteLine("#                                         #");
			Console.WriteLine("###########################################");

			Console.WriteLine ("\n\nAuthor: @_mgp25 - github.com/mgp25 - mgp25.com");
			Console.WriteLine ("________________________________________________");

			if (args.Length < 3) 
			{
				Console.WriteLine ("\n\nUsage: extractPW.exe <username> <password> <ip>");
				Console.WriteLine("\n\nPress any key to exit.");
				Console.ReadKey();
				Environment.Exit (0);
			}

			String path = getPath (args[0], args[1], args[2]);
			getData (path, args[0], args[1], args[2]);

			String data = "";
			try
			{
				data = System.IO.File.ReadAllText(@"Cache.db-wal");
			}
			catch(Exception error) 
			{
				Console.WriteLine ("The file doesn't exist or bad file permissions");
			}
			if(Between (data, "id=", "&lg") != "")
				Console.WriteLine("Identity: " + Between (data, "id=", "&lg"));
			if(Between (data, "pw\":\"", "\",\"type") != "")
				Console.WriteLine("Password: " + Between (data, "pw\":\"", "\",\"type"));
		}
	}
}