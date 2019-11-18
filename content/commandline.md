---
toc: true
title: Commandline
description: Commandline arguments
menu:
    ipfilter:
        parent: ipfilter
        weight: 40
---
# Command Line

{{% note %}}
The command line and its documentation is in a state of flux. Please get in contact with any suggestions or issues for how you want it to work.
{{% /note %}}

## Syntax

`ipfilter.exe [options] [inputs] [outputs] `

### Options

`/silent`

> Launch, run and exit silently. Useful for when automating IPFilter yourself.

`/service`

> Instruct IPFilter to register itself as a Windows service, and start running.

service install
service uninstall
`service` Run as a service
`service install` hugo server



## Supported file formats

.zip, .gzip, .dat, .json


## Inputs

You can specify inputs as URIs / URLs or file system paths (including UNC paths).

The supported file formats are auto-detected and can chain. For example, you can specify a json file that has been zipped. The file will automatically be downloaded, unzipped and then the filters specified inside will then be downloaded.

This allows you to curate your single ipfilter.dat from several disparate sources, such as lists published on the net, your local network or your machine.

## Outputs

### Support output formats

app:<application name> - ut|utorrent, bt|bittorrent, qbt|qbittorrent, auto (all installed apps will be updated), autoscan (aggressively scan for apps, useful for portable versions)
o|out|output:URI/URL/filepath - Outputs / pushes to the destination(s).



You can also output the final list to a network share or push to a web point to publish your combined list for others.

You can output the configuration to a JSON file to parse in next time.
You can also output your sources into your own JSON input file.

## Examples

`ipfilter`

> Downloads the latest default list, and installs it to all detected Bit Torrent applications.

`ipfilter https://servername/ipfilter.gzip \\server\shared\ipfilter.dat o:C:\Temp\ipfilter.dat`

> Downloads and combines two lists from a gzipped-list from the internet and a local network share, then saves it to a local file. 
