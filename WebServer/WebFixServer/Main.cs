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
using GlynnTucker.Cache;
using Newtonsoft.Json;
namespace WebFixServer
{

	class MainClass
	{
		
		public static void Main (string[] args)
		{
			
			
			Filter filter = new Filter();
			
			
            var server = new WebSocketServer("ws://127.0.0.1:8181");  //Initialise websocket server on localhost
            WebSocketSharp.WebSocket sock = new WebSocketSharp.WebSocket("ws://nodejs-projectthoth.rhcloud.com:8000","server");
			
			FleckLog.Level = LogLevel.Debug; //Set websockets to print debugging messages
			sock.Log.Level = WebSocketSharp.LogLevel.Info;
			
			sock.OnMessage += delegate(object sender, WebSocketSharp.MessageEventArgs e) {
				string data = e.Data;
				Console.WriteLine("Data:");
				Console.WriteLine(data);
				if(e.Type == WebSocketSharp.Opcode.Text && data!=null)
				{
					Message msg;
					try
					{
					msg = JsonConvert.DeserializeObject<Message>(data);
					}
					catch(Exception ex)
					{
						Console.WriteLine("Failed to decode message");
						Console.WriteLine("Data:");
						Console.WriteLine(data);
						Console.WriteLine("Exception:");
						Console.WriteLine(ex);
						return;
						
						
					}
						
					if(msg.html == null)
					{
						Console.WriteLine(data);
						return;
					}
					msg.html = filter.Run(msg.html);
					string serialized = JsonConvert.SerializeObject(msg);
					sock.Send(serialized);
				}
			};
			
			sock.Connect();
			
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
			Cache.AddContext("Nodes");
			Cache.AddContext("Website");
			
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
			object output;
			if(Cache.TryGet("Website",text,out output))
			{
				return (string)output;	
				
			}
			   
			            
			
			
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
			
			Cache.AddOrUpdate("Website",text,doc.DocumentNode.InnerHtml);
			
			return doc.DocumentNode.InnerHtml;
		}
		public void Filtered(HtmlTextNode node )
		{
			string original = node.Text;
			object output = node.Text;
			if(Cache.TryGet("Nodes",node.Text,out output))
			{
				node.Text = (string)output;
				return;
				
			}
			
			foreach(Script script in scripts)
			{
				try
				{	
					node.Text = script.CacheRun(node.Text);

				}
				catch(Exception e)
				{
					Console.WriteLine(e); //Display any exceptions thrown by scripts
				}
				
			}
			Console.WriteLine(original + " >> " + node.Text);
			Cache.AddOrUpdate("Nodes",original,node.Text);
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
			if(info.Extension == ".exe")
			{
				NativeScript exe = new NativeScript();
				exe.Load(info);
				scripts.Add(exe);
				
				
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
		string unique_cache_id ;
		public Script()
		{
			unique_cache_id = Path.GetRandomFileName();
			Cache.AddContext(unique_cache_id);
		}
		
		public abstract string Run(string input);
		public abstract void Load(FileInfo info);
		public string CacheRun(string input)
		{
			object result;
			if(!Cache.TryGet(unique_cache_id,input,out result))
			{
				result = Run(input);
				Cache.AddOrUpdate(unique_cache_id,input,result);
				
				
			}
			return (string)result;
			
			
		}
		
		
		
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
			info.RedirectStandardInput = true; //Redirect stdio for Python/C# communication
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
		
			
			object result = ((ObjectHandle)operations.Invoke(filter,input.Clone())).Unwrap();
			return (string)result;
		}
		
		
		
	}
	public class NativeScript : Script
	{
		ProcessStartInfo info;
		
		public override void Load (FileInfo fileinfo)
		{
			info = new ProcessStartInfo(fileinfo.FullName);
			info.WorkingDirectory = fileinfo.DirectoryName;
			info.RedirectStandardInput = true; //Redirect stdio for Program/C# communication
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
	public struct Message
	{
		public string html;
		public string id;
		
		
	}




}
		