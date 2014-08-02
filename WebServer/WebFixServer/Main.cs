//Imports
using System;
using Fleck;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;



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
			List<Thread> threads = new List<Thread>();
			SortedList<int,Replacement> replacements = new SortedList<int,Replacement>();
			string[] segments = text.Split('>'); //Split text by closing html tag

			for(int i = 1;i< segments.Length;i++) //Start at 1 to ignore first html tag
			{

				string segment = segments[i];

				if(segment.Contains('<')&&(!(segment.EndsWith("script")||segment.EndsWith("code")||segment.EndsWith("style")))) //Ignore tags that don't contain visible text
				{
					string plaintext = segment.Substring(0,segment.IndexOf('<')); //Retrieve the text until the next html tag

					if(!string.IsNullOrWhiteSpace(plaintext)) //Check that it isn't empty
					{
                        if (plaintext.Trim().Length > 19) //Ignore small strings (To improve speed because my laptop is slow)
                        {

                            //Create replacement with data required to reconstruct html
                            Replacement r = new Replacement();
                            r.length = segment.Length;
                            r.tail = segment.Substring(segment.IndexOf('<'));
                            r.i = i;
                            r.original = plaintext;

                            //Start filtering the text in a new thread and add it to the list
                            Thread t = new Thread(new ThreadStart(() => { Filtered(r, replacements); }));
                            t.Start();
                            threads.Add(t);
                        }
					}
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

            //Make changes to html
			foreach(Replacement r in replacements.Values)
			{				
                int index = text.IndexOf(r.original);
				text = text.Remove(index,r.length); //Remove old version
				text = text.Insert(index,r.replacement+r.tail); //Replace with altered version	
				Console.WriteLine(r.i +") " + r.original + " >>> " + r.replacement); //Display change on console for debugging
			}
			
			return text;
		}


        //Runs Replacement "r" through all filters
		public void Filtered(Replacement r, SortedList<int,Replacement> replacements )
		{
			string text = r.original;
			
			foreach(ProcessStartInfo script in scripts)
			{
				try
				{
				    Process p =  Process.Start(script); //Run each script and allow it to alter text
				    p.StandardInput.WriteLine(text);
				    text = p.StandardOutput.ReadToEnd();
				}
				catch(Exception e)
				{
					Console.WriteLine(e); //Display any exceptions thrown by scripts
				}
			}

            r.replacement = text; //Save replaced text to replacement
            replacements.Add(r.i, r); //Add replacement to list

			Console.WriteLine("  " + r.original + " >> " + r.replacement+ " queued at: "+ r.i );
			
		}


        //Prepare a script at "location" for usage
		void AddScript(string location)
		{
			FileInfo fileinfo = new FileInfo(location); 
			
			ProcessStartInfo info = new ProcessStartInfo("python.exe",fileinfo.Name);
			info.WorkingDirectory = fileinfo.DirectoryName;
			info.RedirectStandardInput = true; //Redirect stdio for Python/C# communitcation
			info.RedirectStandardOutput = true; //"
			info.UseShellExecute = false;
			scripts.Add (info); //Add information to start script process in its directory
		}
	}


    //Structure for replacement containing all required information to rebuild html
	public struct Replacement
	{
		public int length;
		public string replacement;
		public string tail;
		public string original;
		public int i;
	}
		

}
		