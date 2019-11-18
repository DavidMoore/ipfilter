---
toc: true
title: Architecture
description: IPFilter Architecture
menu:
    ipfilter:
        parent: ipfilter
        weight: 50
---


{{% note %}}
This documentation is a bit out of date and won't be updated for at least a few weeks
{{% /note %}}

The IPFilter updater takes in a set of one or more inputs, and one or more outputs.

IPFilter acquires and merges all of the inputs, and generates the output result.

The merged inputs are then synced to the output(s).

# Inputs

## Input URIs

The input must be resolved to an absolute URI. While the final URI must be absolute, there are shortcuts and abbreviations to make the specified input(s) more concise and readable.

## Input Formats

The input files can be of a couple of formats:

* A raw ipfilter file
* A compressed ipfilter file (zip, gzip)
* A JSON file in the IPFilter Bundle format. This is a special format that allows additional inputs to be specified.

## Bundles

A bundle file is a JSON definition of input(s) and/or output(s).

This facilitates combining complex or large lists of ipfilter input URIs into a small file that can be saved. It can simplify URIs by having a list parent, and consolidate authentication for protected lists.

Because inputs specified in a bundle can be bundles themselves, this allows a nested hierarchy of sources to be defined.

This allows specialized bundles to be created and published on your local network or even publicly.

# Enumerators

# TextEnumerator

The lowest level enumerator, reading in text lines from the specified stream. Binary lines are skipped over.

# UriEnumerator

The highest level enumerator, this takes in a source URI.

It will acquire the contents of the source URI, decompressing if necessary.

If the resulting contents is determined to be a Bundle, the Bundle when then be parsed and each input source passed through to the UriEnumerator.

# JsonEnumerator

The JsonEnumerator finds and iterates over the input sources defined in the passed json file.

Each input source has its URI resolved, then passed through to the UriEnumerator.



`ipfilter`

> Downloads the latest default list, and installs it to all detected Bit Torrent applications.

`ipfilter https://servername/ipfilter.gzip \\server\shared\ipfilter.dat o:C:\Temp\ipfilter.dat`

> Downloads and combines two lists from a gzipped-list from the internet and a local network share, then saves it to a local file. 
