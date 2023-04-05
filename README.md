# ERKON

Hi, I created this platform to control AirCon using microcontroller Arduino through infrared (a.k.a. AC remote control). The idea is to be able to control all Aircon through web application. Things that are controllable are:
- Temperature and humidity
- On and Off

## Problem Statement
There is a building with many Aircon installed and everyday an admin have to check whether all Aircons are off when all the employee have gone home or when the employees leave their room due to outside activity. There is also a policy from the operation department that the Aircon have to be set to maximum 20 degree celcius to maintain the lifetime of the Aircon.
This become a challenge to admin because he/she has to walk to all rooms to ensure the Aircon is off and to ensure the temperature is set according to company policy.

## Solution
I created a platform so admin can control all Aircons in the building through web application. Admin not only able to turn on or off the Aircon, but he/she can also monitor and change the room temperature realtime. Therefore, admin doesn't have to walk to every room any more and make his/her time more efficient.

## Hardware Requirement
1. Arduino NodeMCU Microcontroller
2. Temperature and Humidity Sensor (DHT22)
3. Infrared IR Receiver and IR Transmitter
4. Power Supply 12V
5. Aircon (of course, hehe)

## Other Requirement
1. MQTT Broker Server. I use mosquitto MQTT broker.
2. Wifi. The connection between Arduino and the broker is through wifi

## How to run
1. Microcontroller Program
In "Arduino" folder, there is an .ino file which you can compile and upload to Arduino NodeMCU. In order to do this, you'll have to have knowledge on how to compile the code and upload it to Arduino NodeMCU.
Before compiling, please ensure all needed configuration are setup
2. Web Application
This is .Net core MVC application. You can use Visual Studio 2022 to run the project. Before running it, please ensure the config are defined properly
3. Listener
This is .Net core console application which the task is to listen the updated temperature and humidity from Arduino

I know the description here is far from perfect and I'm sure you'll get confuse :) I will be revisiting this readme from time to time when I have the time (I know, it's rhyme right hehe).

over and out,
Fendy
