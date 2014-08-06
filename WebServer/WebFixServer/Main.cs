//Imports
using System;
using Fleck;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;
using HtmlAgilityPack;



namespace WebFixServer
{

	class MainClass
	{
		
		public static void Main (string[] args)
		{
			
			Filter filter = new Filter();
			FleckLog.Level = LogLevel.Debug; //Set websockets to print debugging messages
			
            var server = new WebSocketServer("ws://127.0.0.1:8181");  //Initialise websocket server on localhost
           
            //Start server
			server.Start(socket =>
                {
                    socket.OnMessage = message =>
                        {
							string html = filter.Run(message); //Run filters on the innerHTML
							socket.Send(html); //Send the altered version
                        };
                });
			
            //Check for 'exit' command
            var input = Console.ReadLine(); 
			
            while (input != "exit")
            {
                input = Console.ReadLine();
            }
		}
	}


	class Filter
	{
		List<ProcessStartInfo> scripts = new List<ProcessStartInfo>(); //List containing all loaded scripts
		
		
		public Filter()
		{
			
            //Iterate through all files in ./Filters/
			foreach(string file in Directory.GetFiles("Filters"))
			{						
						
				if (file.EndsWith(".py")) //If a file in "Filters" ends in .py add it as a script
				{
					try
					{
						AddScript(file);
					}
					catch(Exception e)
					{
					    Console.WriteLine("Failed to add "+ file); //Display any exceptions
					    Console.WriteLine(e);
					}
				}else if (file.EndsWith(".script")) //If a file in "Filters" ends in .script add the location in the file as a script
				{
					try
					{
						string location = File.ReadAllText(file); //Read .py location from .script
						location = location.Replace("\n",""); //Remove newline character

						if(Environment.OSVersion.Platform == System.PlatformID.Unix) //Change windows backslashes to forward slashes
							location = location.Replace("\\","/");

                        if (!File.Exists(location))
                        {
                            throw new FileNotFoundException("Could not find :" + location);
                        }

						AddScript(location);
					}
					catch(Exception e)
					{
                        Console.WriteLine("Failed to add " + file); //Display any exceptions
						Console.WriteLine(e);
					}
				}
			}
		}

		
        //Separates plaintext from html and runs it through filters before adding it back in
		public string Run(string text)
		{
			
			
			var doc = new HtmlAgilityPack.HtmlDocument();
			List<Thread> threads = new List<Thread>();
			doc.LoadHtml(text);
			foreach (HtmlAgilityPack.HtmlTextNode node in doc.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']"))
    		{

				if(node.ParentNode.Name!="script"&&node.Text.Trim().Length>15)
				{
					
					Thread t = new Thread(new ThreadStart(() => { Filtered(node); }));
	                            t.Start();
	                            threads.Add(t);
					
				}
    		}
			            //Rejoin threads with main threads when they are finished
            foreach (Thread t in threads)
            {
                if (t.IsAlive)
                {
                    t.Join();
                }
            }
			
			return doc.DocumentNode.InnerHtml;
		}
		public void Filtered(HtmlTextNode node )
		{
			string original = node.Text;
			
			foreach(ProcessStartInfo script in scripts)
			{
				try
				{
				    Process p =  Process.Start(script); //Run each script and allow it to alter text
				    p.StandardInput.WriteLine(node.Text);
				    node.Text = p.StandardOutput.ReadToEnd();
				}
				catch(Exception e)
				{
					Console.WriteLine(e); //Display any exceptions thrown by scripts
				}
				
			}
			Console.WriteLine(original + " >> " + node.Text);
		}


        //Prepare a script at "location" for usage
		void AddScript(string location)
		{
			FileInfo fileinfo = new FileInfo(location); 
			ProcessStartInfo info ;
			if(Environment.OSVersion.Platform == PlatformID.Unix)
				
				info = new ProcessStartInfo("python3.3",fileinfo.Name);
			else
			{
				info = new ProcessStartInfo("python.exe",fileinfo.Name);
			}
			info.WorkingDirectory = fileinfo.DirectoryName;
			info.RedirectStandardInput = true; //Redirect stdio for Python/C# communitcation
			info.RedirectStandardOutput = true; //"
			info.UseShellExecute = false;
			scripts.Add (info); //Add information to start script process in its directory
		}
	}




}
		