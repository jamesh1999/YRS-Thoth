using System;
using System.Net;
using System.Threading;
using Fleck;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.Text;
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
			
            var server = new WebSocketServer("ws://0.0.0.0:8181");  //initialise websocket server on localhost
           
			server.Start(socket =>
                {
                    socket.OnMessage = message =>
                        {
							string html = filter.Run(message); //Run filters on the innerHTML
							socket.Send(html); //send the altered version
					
                        };
                }); //start server
			
			 
            var input = Console.ReadLine();
			
            while (input != "exit") //check to exit
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
						AddScript(File.ReadAllText(file));
					}
					catch(Exception e)
					{
						Console.WriteLine("Failed to add "+ file);
						Console.WriteLine(e);
					}
				}
				
				
				
				
			}
			
		
			
		

			
			
			
		}
		
		public string Run(string text)
		{
			
			string[] segments = text.Split('>'); //split text by closing html tag
			for(int i = 1;i< segments.Length;i++) //start at 1 to remove first html tag
			{
				string segment = segments[i];
				int segmentIndex = text.IndexOf(segment);
				if(segment.Contains('<')&&(!(segment.EndsWith("script")||segment.EndsWith("code")))) //ensure the section contains an opening html tag and does not end with the tag <script> or <code>
				{
					string plaintext = segment.Substring(0,segment.IndexOf('<')); //retrieve the plaintext before the htmltag
					if(!string.IsNullOrWhiteSpace(plaintext))//check it isn't empty
					{
						plaintext = Filtered(plaintext);
						
					}
					text = text.Remove(segmentIndex,segment.Length);
					text = text.Insert(segmentIndex,plaintext+ segment.Substring(segment.IndexOf('<'))); //replace with altered version
				}
			}
			return text;
		}
		public string Filtered(string text)
		{
			
			
			foreach(ProcessStartInfo script in scripts)
			{
				try
				{
				Process p =  Process.Start(script);
				p.StandardInput.Write(text);
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
			
			ProcessStartInfo info = new ProcessStartInfo("python3.3",location);
					info.WorkingDirectory = Directory.GetCurrentDirectory();
					info.RedirectStandardInput = true;
					info.RedirectStandardOutput = true;
					info.UseShellExecute = false;
					scripts.Add (info);
			
		}
			       
		
		
		
	}
	

}
		