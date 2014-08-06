//Imports
using System;
using Fleck;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;
using HtmlAgilityPack;
using IronPython.Compiler;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Runtime.Remoting;

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
		List<Script> scripts = new List<Script>(); //List containing all loaded scripts
		const int MaxThreads = 100;
		
		public Filter()
		{
			
            //Iterate through all files in ./Filters/
			foreach(string file in Directory.GetFiles("Filters"))
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

				if(node.ParentNode.Name!="script"&&node.Text.Trim().Length>17)
				{
					if(threads.Count<MaxThreads)
					{
					Thread t = null;
				    t = new Thread(new ThreadStart(() => { Filtered(node);threads.Remove(t); }));
	                t.Start();
	                threads.Add(t);
					}
					else
					{
						Filtered(node);
						
						
					}
				}
    		}
			            //Rejoin threads with threads when they are finished
            while(threads.Count>0)
			{
				try
				{
					if(threads[0].IsAlive)
						threads[0].Join();
					else
						threads.Remove(threads[0]);
					
					
				}
				catch(Exception)
				{
					
					
				}
				
				
				
			}
			
			return doc.DocumentNode.InnerHtml;
		}
		public void Filtered(HtmlTextNode node )
		{
			string original = node.Text;
			
			foreach(Script script in scripts)
			{
				try
				{	
					node.Text = script.Run(node.Text);

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
			if(Environment.OSVersion.Platform == System.PlatformID.Unix)
						location = location.Replace("\\","/");
			FileInfo info = new FileInfo(location);
			
			if(info.Extension == ".py")
			{
				PythonScript py = new PythonScript();
				py.Load(info);
				scripts.Add(py);
				
			}
			if(info.Extension == ".ipy")
			{
				IronPythonScript ipy = new IronPythonScript();
				ipy.Load(info);
				scripts.Add(ipy);
				
			}
			if(info.Extension == ".script")//If a file in "Filters" ends in .script add the location in the file as a script
			{
				try
				{
					string newlocation = File.ReadAllText(info.FullName); //Read .py location from .script
					location = location.Replace("\n",""); //Remove newline character
					
						
                    if (!File.Exists(location))
                    {
                        throw new FileNotFoundException("Could not find :" + newlocation);
                    }

					AddScript(newlocation);
				}
				catch(Exception e)
				{
                    Console.WriteLine("Failed to add " + info.FullName ); //Display any exceptions
					Console.WriteLine(e);
				}
			
				
				
			}
			if(info.Extension == ".redirect")//If a file in "Filters" ends in .script add the location in the file as a script
			{
				
				ScriptEngine engine = Python.CreateEngine();
				var scope = engine.CreateScope();
				engine.CreateScriptSourceFromFile(info.FullName).Execute(scope);
				string redirect = engine.CreateOperations().Invoke(scope.GetVariable("Redirect"));
				AddScript(redirect);
				
			}
			
		}
	}
	public abstract class Script
	{
		public abstract string Run(string input);
		public abstract void Load(FileInfo info);
		
		
		
		
	}
	public class PythonScript : Script
	{
		ProcessStartInfo info;
		
		public override void Load (FileInfo fileinfo)
		{
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
		}
		
		public override string Run (string input)
		{
			 Process p =  Process.Start(info); //Run each script and allow it to alter text
			 p.StandardInput.WriteLine(input);
			 return p.StandardOutput.ReadToEnd();
		}
		
		
		
	}
	public class IronPythonScript : Script
	{
		ObjectHandle filter;
		ObjectOperations operations;
		public override void Load (FileInfo info)
		{
			CompiledCode script;
			ScriptScope scope;
			ScriptEngine engine = Python.CreateEngine(AppDomain.CreateDomain("Script Sandbox"));
			scope = engine.CreateScope();
			ScriptSource source = engine.CreateScriptSourceFromFile(info.FullName);
			script = source.Compile();
			script.Execute(scope);
			if(scope.TryGetVariableHandle("Filter",out filter))
			{
				operations = engine.CreateOperations();
				
			}
			else
			{
				throw new InvalidDataException("Could not find Filter method");
				
				
			}
			
			
			
		}
		public override string Run (string input)
		{
		
			
			object result = ((ObjectHandle)operations.Invoke(filter,input)).Unwrap();
			return (string)result;
		}
		
		
		
	}




}
		