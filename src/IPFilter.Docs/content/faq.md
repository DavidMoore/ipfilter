---
title: "FAQ"
description: "Frequently Asked Questions"
ur: "/faq/"
---

### What is IPFilter Updater?

It's a small Windows application that will download and update an ipfilter list for use in Bit Torrent clients.

### What Bit Torrent clients are supported?

Currently uTorrent, BitTorrent and qBittorrent are supported out of the box, but you can also configure other applications to pick up the downloaded list.

### What is an ipfilter?

An ipfilter list is a simple (but usually very large) list of addresses and ranges of malicious peers that will transmit junk data (i.e. anti-P2P) or peers that may be intrusive on your net privacy.

### Why do I need an ipfilter?

For privacy and more reliable peers

### Why do I need IPFilter Updater?

It will make it easier to download a list, and keep it up to date

### Where does the list come from?

I do not maintain any of the lists.

I try to keep a variety of third party list options available, but the availability and quality of free, up to date lists seems to be dwindling.

### Where does the filter get downloaded to?

The ipfilter.dat is downloaded to your local user profile in a directory called IPFilter i.e. `%LocalAppData%\IPFilter\ipfilter.dat`

For example, if your username is Bob and you were on Windows 7 or higher, then the location by default would resolve to `C:\Users\Bob\AppData\Local\IPFilter\ipfilter.dat`