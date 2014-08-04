YRS-Thoth
=========
  Project Thoth is the first step towards eradication Internet slang. By reading less of it we hope that people will post less themselves thus reducing the amount that we have to see cluttering up our web browsers.

Install
=========
  Currently the installation is rather complicated as the web server needs to be run locally at the moment. In the future, this may change. For now, here is a step by step guide to installing Project Thoth

  1. Download and extract the zip folder
  2. Download and install python 3.4.1
  3. From the directory where Python is installed copy python.exe
  4. Paste python.exe into YRS-Thoth/WebServer/bin/Release/
  5. Next, open Firefox and navigate to the Add-Ons window
  6. Click the settings inco next to the search bar and select "Install add-on from file"
  7. Install the file YRS-Thoth/Client/thothproxy.xpi
  8. Now use Thoth

Usage
=========
  To use Project Thoth you have to run the webserver (YRS-Thoth/WebServer/bin/Release/WebFixServer.exe). If this is running and the add-on is enabled (by clicking the Thoth icon in the top-right hand corner of the screen so that it turns light-blue), you should be able to navigate to any non-HTTPS:// website and Project Thoth will automatically fix both spelling and grammar. On slower machines this may take a while but, in the meantime, the old website will be displayed. Here is a good website on which to test Project Thoth: http://al153.github.io/bad_grammar

To-Do
=========
-Add an installer
-Compile the standard python scripts
-Fix the javascript bug
