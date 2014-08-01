using System;
using Fleck;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace WebFixServer
{
	class MainClass
	{
		
		public static void Main (string[] args)
		{
			
			Filter filter = new Filter();
			FleckLog.Level = LogLevel.Debug; //Set websockets to print debugging messages
			
            var server = new WebSocketServer("ws://127.0.0.1:8181");  //Initialise websocket server on localhost
           
			server.Start(socket =>
                {
                    socket.OnMessage = message =>
                        {
							string html = filter.Run(message); //Run filters on the innerHTML
							socket.Send(html); //Send the altered version
					
                        };
                }); //Start server
			
            var input = Console.ReadLine();
			
            while (input != "exit") //Check to exit
            {
                input = Console.ReadLine();
            }
			
		}

	}


	class Filter
	{
		List<ProcessStartInfo> scripts = new List<ProcessStartInfo>();
		
		public Filter()
		{
			
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
					    Console.WriteLine("Failed to add "+ file);
					    Console.WriteLine(e);
					}
				}else if (file.EndsWith(".script")) //If a file in "Filters" ends in .script add the location in the file as a script
				{
					try
					{
						string location = File.ReadAllText(file);
						location = location.Replace("\n","");//remove automatically added '\n'
						if(!File.Exists(location))
							throw new FileNotFoundException("Could not find :" + location);
						AddScript(location);
					}
					catch(Exception e)
					{
						Console.WriteLine("Failed to add "+ file);
						Console.WriteLine(e);
					}
				}
				
			}
			
		}

		
        //Separates plaintext from html and runs it through filters before adding it back in
		public string Run(string text)
		{
			
			string[] segments = text.Split('>'); //Split text by closing html tag

			for(int i = 1;i< segments.Length;i++) //Start at 1 to remove first html tag
			{
				string segment = segments[i];

				int segmentIndex = text.IndexOf(segment);

				if(segment.Contains('<')&&(!(segment.EndsWith("script")||segment.EndsWith("code")))) //Ensure the section contains an opening html tag and does not end with the tag <script> or <code>
				{
					string plaintext = segment.Substring(0,segment.IndexOf('<')); //Retrieve the plaintext before the htmltag
					if(!string.IsNullOrWhiteSpace(plaintext)) //Check it isn't empty
					{
						plaintext = Filtered(plaintext); //Filter text
					}
					text = text.Remove(segmentIndex,segment.Length);
					text = text.Insert(segmentIndex,plaintext+ segment.Substring(segment.IndexOf('<'))); //Replace with altered version
				}
			}
			return text;
		}


        //Runs "text" through all filters
		public string Filtered(string text)
		{
			
			
			foreach(ProcessStartInfo script in scripts)
			{
				try
				{
				    Process p =  Process.Start(script);//Run each script and allow it to alter text
				    p.StandardInput.WriteLine(text);
				    text = p.StandardOutput.ReadToEnd();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
				}
				
			}
			return text;
			
		}


		void AddScript(string location)
		{
			FileInfo fileinfo = new FileInfo(location); 
			
			ProcessStartInfo info = new ProcessStartInfo("python3.3",fileinfo.Name);
			info.WorkingDirectory = fileinfo.DirectoryName;
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			scripts.Add (info); //Add information to start script process in its directory
			
		}
		
	}

}
		