XBeeMotion2Cloud
================

Send motion sensor data from XBee to Azure Cloud


The files in this repository contain code to send motion sensor data from a passive IR sensor, to an XBee wireless
transmitter, to a serial port on a PC and finally to a web service in Azure. Additional documentation to come in future 
updates.

The files begining with 'Azure' are the files running in an Azure worker role.  The Azure worker role exposes a 
web service that allows a separate application (see below) to send data to the cloud.

The other file is a seperate application that does the bulk of the work of receiving data from the XBee sensor and 
decoding the data packets.
