mgmtapplauncher2
================

This is a small application I started writing while learning the basics of C# and WPF. It launches arbitrary commands when provided with a URI in the form of:

    protocol://IP/

It was meant as a small tool to supplement my company's [Nagios](http://www.nagios.org/) installation - I wrote a small patch to display pretty management links next to the monitored devices. Unfortunately, manually managing protocol handlers in Windows is cumbersome, so I've come up with an application which is able to update the user's registry to install itself as a protocol handler and then automatically decides which application should be launched based on the protocol name provided in the URI passed as a command line argument.

The digit *2* in the name is due to the fact that I originally implemented this application in pure C/WinAPI, but it used static program paths and was pretty much useless.
