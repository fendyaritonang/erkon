/*
 Basic ESP8266 MQTT example

 This sketch demonstrates the capabilities of the pubsub library in combination
 with the ESP8266 board/library.

 It connects to an MQTT server then:
  - publishes "hello world" to the topic "outTopic" every two seconds
  - subscribes to the topic "inTopic", printing out any messages
    it receives. NB - it assumes the received payloads are strings not binary
  - If the first character of the topic "inTopic" is an 1, switch ON the ESP Led,
    else switch it off

 It will reconnect to the server if the connection is lost using a blocking
 reconnect function. See the 'mqtt_reconnect_nonblocking' example for how to
 achieve the same result without blocking the main loop.

 To install the ESP8266 board, (using Arduino 1.6.4+):
  - Add the following 3rd party board manager under "File -> Preferences -> Additional Boards Manager URLs":
       http://arduino.esp8266.com/stable/package_esp8266com_index.json
  - Open the "Tools -> Board -> Board Manager" and click install for the ESP8266"
  - Select your ESP8266 in "Tools -> Board"

*/

#include <ESP8266WiFi.h>
#include <PubSubClient.h>

#include <Arduino.h>
#include <IRremoteESP8266.h>
#include <IRsend.h>

#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <DHT_U.h>

// *** Unit Code - this can be generated using guid generation tool ***
const char* unitCode = "58e0585e-feeb-44d2-800c-8db1e6183052";

// *** Temperature and humidity Sensor ***
#define DHTPIN 2 //D4
#define DHTTYPE    DHT22
DHT_Unified dht(DHTPIN, DHTTYPE);
uint32_t delayMS;
char temperature[5];
char humidity[5];
float lastTemperature;
float lastHumidity;
// **************************

// *** AC ***
const uint16_t kIrLed = 4;  // ESP8266 GPIO pin to use. Recommended: 4 (D2).
IRsend irsend(kIrLed);  // Set the GPIO to be used to sending the message.

// Raw data of turning off the aircon
uint16_t acOff[139] = {9060, 4446,  670, 1662,  644, 562,  646, 560,  642, 562,  646, 1660,  672, 1634,  672, 534,  650, 556,  676, 530,  676, 1630,  678, 528,  676, 530,  676, 530,  678, 526,  678, 528,  688, 518,  702, 502,  702, 504,  702, 504,  702, 504,  702, 502,  702, 1604,  702, 504,  702, 504,  702, 504,  700, 506,  690, 516,  700, 506,  674, 1608,  698, 532,  674, 1608,  698, 532,  670, 536,  668, 1612,  696, 514,  690, 19946,  694, 512,  668, 1636,  694, 1612,  694, 514,  692, 514,  668, 538,  690, 514,  692, 514,  692, 514,  690, 516,  666, 538,  668, 538,  666, 538,  666, 1640,  668, 538,  666, 540,  666, 540,  666, 538,  668, 540,  664, 542,  664, 542,  664, 542,  664, 542,  662, 542,  662, 544,  662, 564,  642, 564,  642, 564,  640, 1666,  642, 1666,  640, 1666,  640, 1666,  638};

// Raw data of turning on the aircon
uint16_t acOn[139] = {9034, 4444,  696, 1610,  698, 534,  670, 508,  698, 1606,  698, 1610,  698, 1610,  670, 538,  692, 508,  672, 536,  670, 1634,  698, 508,  694, 538,  644, 536,  672, 536,  684, 520,  696, 508,  696, 514,  666, 536,  694, 510,  694, 510,  670, 536,  670, 1636,  694, 1614,  694, 512,  692, 512,  694, 512,  670, 536,  668, 538,  670, 1638,  668, 538,  690, 1618,  668, 536,  666, 538,  668, 1638,  668, 538,  668, 19972,  668, 538,  690, 1616,  666, 1640,  668, 538,  666, 538,  666, 538,  666, 540,  666, 540,  664, 542,  664, 542,  664, 542,  664, 542,  664, 564,  642, 1646,  662, 544,  662, 564,  642, 564,  640, 566,  640, 564,  642, 564,  640, 566,  640, 566,  640, 566,  638, 566,  638, 568,  638, 568,  638, 568,  636, 570,  632, 1676,  632, 1676,  632, 1676,  630, 574,  632};
// *********************

// Raw data of temperature
uint16_t temperature30[139] = {9030, 4446,  694, 512,  698, 1636,  642, 536,  698, 1636,  642, 1664,  642, 536,  698, 508,  698, 508,  698, 506,  698, 1636,  642, 1664,  644, 1664,  642, 536,  698, 506,  698, 508,  698, 508,  698, 508,  698, 508,  698, 506,  698, 506,  698, 508,  698, 508,  698, 1636,  642, 536,  698, 508,  698, 506,  698, 508,  696, 508,  698, 1634,  642, 538,  698, 1636,  642, 536,  698, 508,  698, 1662,  642, 536,  698, 19942,  670, 536,  698, 508,  698, 508,  696, 508,  698, 508,  698, 506,  698, 510,  696, 508,  696, 508,  698, 506,  698, 508,  696, 508,  698, 508,  698, 1636,  642, 536,  698, 508,  696, 510,  698, 508,  698, 508,  698, 506,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 510,  698, 508,  698, 1636,  642, 538,  696};
uint16_t temperature29[139] = {9032, 4446,  668, 538,  666, 1664,  644, 536,  668, 1666,  642, 1664,  642, 540,  668, 540,  666, 536,  668, 1666,  642, 538,  666, 1664,  642, 1666,  640, 540,  666, 540,  664, 540,  666, 538,  668, 538,  666, 564,  642, 564,  642, 564,  642, 564,  642, 542,  688, 1640,  642, 542,  664, 540,  664, 540,  664, 538,  668, 538,  690, 1642,  642, 538,  668, 1664,  642, 564,  640, 540,  666, 1664,  644, 564,  640, 19970,  668, 540,  666, 540,  666, 538,  666, 540,  666, 540,  664, 540,  666, 538,  666, 540,  666, 564,  640, 540,  666, 542,  664, 542,  664, 540,  666, 1666,  640, 566,  640, 566,  642, 564,  640, 564,  640, 564,  642, 564,  642, 564,  640, 566,  640, 564,  640, 566,  640, 570,  636, 564,  642, 564,  640, 566,  640, 1664,  644, 1664,  644, 562,  644, 562,  644};
uint16_t temperature28[139] = {9060, 4444,  668, 536,  698, 1634,  644, 536,  696, 1634,  646, 1662,  646, 538,  696, 510,  696, 510,  694, 512,  696, 510,  696, 1634,  646, 1660,  646, 536,  698, 508,  694, 510,  696, 510,  698, 508,  694, 510,  692, 514,  696, 510,  696, 510,  696, 510,  668, 1660,  646, 538,  694, 510,  692, 514,  696, 508,  672, 534,  694, 1636,  670, 512,  668, 1662,  644, 536,  670, 536,  670, 1660,  646, 538,  696, 19942,  668, 536,  698, 508,  694, 512,  696, 510,  694, 512,  696, 508,  696, 510,  696, 510,  696, 510,  696, 508,  696, 508,  696, 510,  698, 508,  698, 1634,  646, 536,  696, 508,  698, 508,  698, 508,  698, 508,  696, 508,  698, 508,  698, 508,  696, 508,  698, 508,  698, 508,  696, 508,  698, 508,  694, 512,  696, 508,  672, 1660,  646, 538,  668, 540,  666};
uint16_t temperature27[139] = {9034, 4446,  692, 514,  698, 1636,  642, 538,  696, 1636,  644, 1664,  642, 536,  698, 508,  698, 508,  700, 1636,  648, 1658,  642, 536,  698, 1636,  642, 536,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 506,  700, 506,  698, 508,  698, 1636,  642, 536,  698, 508,  698, 508,  696, 508,  696, 510,  696, 1636,  642, 538,  698, 1636,  642, 538,  698, 508,  698, 1636,  644, 536,  698, 19942,  670, 536,  698, 508,  698, 508,  698, 508,  698, 508,  694, 510,  696, 510,  670, 534,  668, 536,  670, 538,  666, 542,  664, 566,  640, 566,  640, 1666,  620, 586,  640, 564,  644, 562,  670, 536,  672, 534,  672, 532,  674, 532,  672, 534,  672, 512,  694, 508,  696, 510,  696, 510,  694, 510,  696, 510,  694, 1614,  694, 512,  694, 512,  694, 514,  692};
uint16_t temperature26[139] = {9032, 4446,  692, 514,  698, 1634,  644, 534,  698, 1636,  642, 1664,  642, 536,  696, 508,  698, 508,  700, 506,  698, 1636,  652, 526,  698, 1634,  642, 536,  698, 508,  696, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  696, 508,  698, 508,  698, 1636,  640, 538,  698, 506,  698, 508,  698, 506,  698, 508,  698, 1636,  642, 536,  698, 1636,  644, 536,  700, 506,  698, 1636,  642, 536,  698, 19940,  694, 514,  698, 508,  698, 508,  698, 508,  698, 508,  698, 506,  698, 508,  698, 508,  698, 508,  698, 508,  696, 510,  696, 508,  698, 508,  698, 1662,  642, 536,  698, 508,  696, 508,  698, 508,  696, 508,  698, 508,  698, 508,  696, 508,  698, 508,  698, 508,  696, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  696, 510,  696};
uint16_t temperature25[139] = {9062, 4442,  694, 512,  692, 1638,  672, 510,  668, 1662,  672, 1634,  672, 512,  694, 510,  694, 512,  666, 1662,  670, 514,  668, 538,  692, 1636,  672, 512,  692, 514,  666, 538,  666, 538,  668, 538,  694, 512,  692, 514,  692, 514,  668, 538,  692, 514,  692, 1636,  670, 512,  694, 512,  692, 512,  668, 538,  692, 512,  668, 1662,  646, 538,  676, 1652,  646, 536,  692, 512,  668, 1662,  644, 540,  666, 19974,  666, 538,  668, 536,  668, 538,  692, 514,  666, 538,  668, 536,  668, 536,  670, 538,  668, 538,  668, 540,  664, 564,  642, 564,  642, 564,  620, 1682,  648, 560,  646, 560,  648, 558,  672, 512,  694, 510,  694, 510,  694, 512,  696, 510,  694, 510,  668, 538,  694, 512,  668, 536,  668, 536,  668, 538,  668, 1640,  666, 1642,  666, 1642,  666, 1640,  666};
uint16_t temperature24[139] = {9060, 4444,  668, 536,  698, 1636,  642, 536,  698, 1636,  658, 1650,  642, 536,  698, 508,  698, 508,  696, 506,  700, 508,  698, 508,  698, 1636,  644, 536,  696, 508,  698, 508,  696, 510,  694, 512,  696, 510,  690, 514,  698, 508,  692, 512,  698, 508,  694, 1638,  642, 538,  696, 508,  696, 508,  696, 510,  696, 510,  696, 1636,  642, 536,  696, 1638,  644, 534,  670, 538,  668, 1664,  642, 538,  692, 19948,  670, 538,  668, 538,  666, 538,  670, 536,  668, 540,  666, 538,  668, 538,  668, 540,  666, 540,  666, 538,  666, 540,  666, 540,  666, 564,  640, 1666,  644, 538,  666, 564,  642, 564,  640, 566,  640, 564,  640, 564,  642, 564,  640, 542,  664, 564,  640, 564,  642, 564,  642, 564,  644, 562,  644, 560,  646, 560,  670, 1636,  672, 1634,  674, 1634,  674};
uint16_t temperature23[139] = {9060, 4444,  668, 536,  696, 1636,  642, 536,  696, 1636,  644, 1664,  644, 536,  694, 510,  696, 510,  694, 1640,  642, 1666,  640, 1666,  642, 536,  672, 534,  686, 518,  694, 510,  670, 536,  668, 536,  672, 536,  668, 536,  680, 526,  668, 534,  672, 534,  672, 1662,  642, 534,  670, 536,  670, 536,  670, 536,  668, 536,  668, 1664,  642, 536,  670, 1664,  644, 536,  668, 536,  670, 1664,  642, 536,  670, 19970,  668, 538,  668, 536,  668, 536,  670, 536,  670, 538,  668, 536,  668, 538,  668, 536,  670, 538,  668, 536,  668, 536,  668, 540,  666, 538,  668, 1664,  642, 538,  668, 538,  668, 538,  668, 538,  668, 538,  668, 538,  666, 538,  668, 538,  668, 538,  666, 538,  666, 540,  666, 542,  664, 540,  666, 540,  664, 1664,  642, 566,  640, 1664,  644, 1664,  644};
uint16_t temperature22[139] = {9032, 4472,  666, 540,  664, 1664,  644, 540,  666, 1664,  642, 1664,  644, 538,  666, 538,  666, 538,  666, 540,  666, 1664,  644, 1662,  644, 540,  666, 538,  666, 538,  666, 540,  666, 540,  666, 540,  666, 540,  666, 540,  666, 538,  666, 538,  668, 538,  666, 1662,  644, 538,  666, 538,  666, 542,  664, 542,  666, 538,  666, 1662,  644, 540,  666, 1662,  646, 540,  666, 540,  666, 1662,  646, 538,  666, 19974,  666, 540,  666, 540,  666, 538,  668, 538,  666, 538,  666, 540,  668, 538,  668, 538,  666, 540,  666, 540,  690, 516,  666, 538,  668, 538,  666, 1662,  650, 534,  670, 534,  668, 540,  666, 540,  664, 562,  642, 564,  622, 580,  644, 562,  644, 560,  648, 558,  648, 536,  692, 512,  692, 512,  692, 512,  694, 510,  692, 512,  692, 1616,  668, 1640,  666};
uint16_t temperature21[139] = {9062, 4444,  666, 536,  700, 1636,  642, 536,  698, 1636,  642, 1666,  642, 536,  698, 508,  698, 506,  700, 1634,  642, 538,  698, 1636,  644, 534,  698, 508,  698, 508,  696, 508,  698, 508,  696, 508,  700, 508,  696, 508,  698, 508,  698, 508,  698, 508,  698, 1636,  666, 512,  698, 508,  696, 508,  698, 508,  698, 508,  698, 1636,  642, 538,  698, 1636,  640, 538,  698, 508,  698, 1636,  642, 536,  698, 19942,  670, 536,  696, 508,  698, 508,  696, 508,  698, 506,  698, 508,  698, 508,  696, 508,  696, 510,  696, 510,  668, 538,  668, 538,  668, 540,  664, 1666,  640, 566,  618, 588,  620, 586,  620, 586,  644, 562,  644, 560,  672, 534,  674, 532,  674, 532,  674, 532,  672, 510,  694, 510,  696, 510,  694, 510,  694, 1612,  696, 1612,  694, 512,  696, 1612,  696};
uint16_t temperature20[139] = {9032, 4472,  668, 538,  668, 1662,  646, 538,  668, 1662,  646, 1662,  646, 538,  666, 540,  666, 538,  668, 538,  666, 540,  666, 1664,  668, 516,  666, 540,  666, 538,  668, 538,  666, 540,  666, 538,  666, 538,  668, 538,  666, 538,  668, 538,  668, 538,  666, 1662,  668, 516,  666, 538,  666, 540,  666, 540,  666, 538,  668, 1662,  644, 540,  666, 1662,  644, 540,  666, 538,  666, 1662,  644, 540,  666, 19972,  666, 538,  668, 538,  666, 540,  666, 540,  666, 538,  666, 538,  668, 538,  666, 540,  666, 540,  668, 538,  666, 538,  668, 538,  668, 538,  668, 1662,  644, 538,  666, 540,  668, 538,  668, 538,  668, 538,  666, 540,  692, 514,  666, 538,  666, 538,  666, 538,  692, 514,  690, 514,  666, 538,  668, 538,  668, 536,  668, 1662,  670, 514,  668, 1660,  674};
uint16_t temperature19[139] = {9032, 4446,  668, 536,  698, 1636,  644, 536,  700, 1634,  644, 1664,  642, 536,  698, 508,  698, 506,  698, 1636,  642, 1666,  642, 536,  698, 508,  698, 508,  698, 508,  698, 508,  698, 506,  698, 508,  698, 508,  700, 506,  698, 508,  698, 508,  698, 506,  698, 1636,  642, 536,  698, 508,  698, 506,  698, 508,  698, 508,  698, 1636,  642, 536,  698, 1636,  646, 534,  698, 508,  696, 1636,  642, 536,  698, 19942,  668, 536,  698, 506,  698, 508,  698, 508,  698, 508,  696, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  696, 1636,  642, 538,  698, 508,  698, 506,  698, 508,  696, 508,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  696, 508,  698, 508,  696, 508,  696, 510,  696, 1636,  642, 536,  694, 510,  692, 1642,  644};
uint16_t temperature18[139] = {9034, 4472,  666, 540,  668, 1662,  644, 540,  666, 1662,  646, 1662,  644, 540,  666, 540,  666, 540,  666, 540,  666, 1664,  644, 540,  666, 538,  666, 540,  666, 540,  666, 540,  664, 540,  666, 540,  666, 538,  666, 540,  666, 540,  666, 540,  666, 540,  666, 1662,  644, 540,  666, 540,  666, 540,  666, 540,  666, 540,  664, 1664,  644, 540,  666, 1662,  644, 540,  666, 540,  666, 1664,  644, 540,  666, 19974,  666, 538,  666, 540,  666, 540,  664, 540,  664, 540,  666, 540,  666, 540,  664, 540,  668, 538,  664, 540,  666, 540,  668, 540,  664, 540,  666, 1662,  644, 540,  666, 540,  666, 538,  666, 540,  666, 540,  666, 540,  666, 540,  666, 540,  666, 540,  666, 540,  666, 540,  668, 540,  666, 538,  666, 540,  666, 540,  666, 540,  666, 540,  668, 1660,  646};
uint16_t temperature17[139] = {9060, 4444,  690, 514,  698, 1636,  642, 536,  698, 1636,  642, 1666,  642, 538,  696, 508,  698, 506,  698, 1636,  644, 536,  698, 506,  698, 506,  698, 506,  700, 506,  698, 508,  698, 508,  698, 506,  698, 508,  696, 508,  698, 508,  698, 508,  696, 508,  698, 1636,  642, 536,  696, 508,  698, 508,  698, 508,  696, 508,  700, 1636,  642, 536,  698, 1636,  644, 534,  698, 508,  696, 1638,  642, 536,  698, 19942,  668, 536,  696, 508,  698, 506,  696, 512,  696, 508,  698, 510,  696, 510,  692, 512,  696, 508,  694, 512,  692, 512,  694, 512,  694, 512,  692, 1642,  642, 536,  670, 536,  692, 512,  670, 536,  668, 538,  668, 536,  670, 536,  668, 536,  668, 536,  668, 536,  668, 536,  670, 536,  670, 538,  668, 536,  668, 1664,  642, 1666,  642, 1666,  642, 564,  640};
uint16_t temperature16[139] = {9008, 4498,  670, 536,  698, 1636,  644, 536,  698, 1634,  644, 1662,  642, 538,  696, 506,  698, 508,  698, 508,  698, 508,  698, 508,  698, 508,  696, 510,  696, 510,  668, 538,  692, 512,  692, 514,  692, 512,  668, 538,  694, 512,  666, 540,  692, 514,  668, 1662,  646, 538,  692, 514,  694, 512,  668, 538,  666, 538,  692, 1638,  668, 514,  668, 1662,  644, 540,  666, 540,  666, 1662,  644, 540,  668, 19972,  666, 540,  666, 538,  666, 538,  666, 540,  666, 538,  670, 536,  664, 540,  666, 540,  666, 540,  666, 540,  666, 540,  666, 540,  664, 540,  668, 1662,  646, 540,  666, 540,  666, 538,  668, 538,  666, 538,  666, 540,  664, 540,  668, 538,  666, 540,  666, 540,  668, 538,  666, 540,  664, 540,  664, 540,  668, 538,  666, 1662,  646, 1662,  646, 538,  668};

// *** wifi and mqtt ***
const char* ssid = "<change to your wifi ssid>";
const char* password = "<change to your wifi password>";
const char* mqtt_server = "<change to ip address of your mqtt server>";
const char* mqtt_username = "<change to mqtt username>";
const char* mqtt_password = "<change to password of your mqtt username>";

WiFiClient espClient;
PubSubClient client(espClient);
long lastMsg = 0;
char msg[50];
int value = 0;

char payloadMessage[10];
char payloadOn[2] = {'0', '1'};
char payloadOff[2] = {'0', '0'};
char payloadTemp16[2] = {'1', '6'};
char payloadTemp17[2] = {'1', '7'};
char payloadTemp18[2] = {'1', '8'};
char payloadTemp19[2] = {'1', '9'};
char payloadTemp20[2] = {'2', '0'};
char payloadTemp21[2] = {'2', '1'};
char payloadTemp22[2] = {'2', '2'};
char payloadTemp23[2] = {'2', '3'};
char payloadTemp24[2] = {'2', '4'};
char payloadTemp25[2] = {'2', '5'};
char payloadTemp26[2] = {'2', '6'};
char deviceInfoPayload[50];
// *********************

void setup_wifi() {

  delay(10);
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);

  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  randomSeed(micros());

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
  
  // Turning off LED
  digitalWrite(BUILTIN_LED, HIGH);
}

void callback(char* topic, byte* payload, unsigned int length) {
  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("] ");
  for (int i = 0; i < length; i++) {    
	  payloadMessage[i] = (char)payload[i];
  }  
  Serial.print(payloadMessage);
  Serial.println();
  
  // Switch on the LED if an 1 was received as first character  
  if (checkPayload(payloadMessage, payloadOn, 2)) 
  {
	  irsend.sendRaw(acOn, 139, 38);  // Turn on the aircon
    Serial.println("Turn on AC");
  } 
  else if (checkPayload(payloadMessage, payloadOff, 2))
  {
	  irsend.sendRaw(acOff, 139, 38); // Turn off the aircon
    Serial.println("Turn off AC");
  }
  else if (checkPayload(payloadMessage, payloadTemp16, 2))
  {
	  //Change temperature to 16 degree
    irsend.sendRaw(temperature16, 139, 38);
    Serial.println("Change temperature to 16 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp17, 2))
  {
	  //Change temperature to 17 degree
    irsend.sendRaw(temperature17, 139, 38);
    Serial.println("Change temperature to 17 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp18, 2))
  {
	  //Change temperature to 18 degree
    irsend.sendRaw(temperature18, 139, 38);
    Serial.println("Change temperature to 18 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp19, 2))
  {
	  //Change temperature to 19 degree
    irsend.sendRaw(temperature19, 139, 38);
    Serial.println("Change temperature to 19 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp20, 2))
  {
	  //Change temperature to 20 degree
    irsend.sendRaw(temperature20, 139, 38);
    Serial.println("Change temperature to 20 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp21, 2))
  {
	  //Change temperature to 21 degree
    irsend.sendRaw(temperature21, 139, 38);
    Serial.println("Change temperature to 21 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp22, 2))
  {
	  //Change temperature to 22 degree
    irsend.sendRaw(temperature22, 139, 38);
    Serial.println("Change temperature to 22 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp23, 2))
  {
	  //Change temperature to 23 degree
    irsend.sendRaw(temperature23, 139, 38);
    Serial.println("Change temperature to 23 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp24, 2))
  {
	  //Change temperature to 24 degree
    irsend.sendRaw(temperature24, 139, 38);
    Serial.println("Change temperature to 24 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp25, 2))
  {
	  //Change temperature to 25 degree
    irsend.sendRaw(temperature25, 139, 38);
    Serial.println("Change temperature to 25 degree");
  }
  else if (checkPayload(payloadMessage, payloadTemp26, 2))
  {
	  //Change temperature to 26 degree
    irsend.sendRaw(temperature26, 139, 38);
    Serial.println("Change temperature to 26 degree");
  }
}

bool checkPayload(char* pload, char* code, int codeLength){
  for (byte i = 0; i < codeLength; i++) {
    if (pload[i] != code[i]){
      return false;
    }
  }
  return true;
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Create a random client ID
    String clientId = "ESP8266Client-";
    clientId += String(random(0xffff), HEX);
    // Attempt to connect
    if (client.connect(clientId.c_str(), mqtt_username, mqtt_password)) {
      Serial.println("connected");
      client.subscribe(unitCode);
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void setup() {
  pinMode(BUILTIN_LED, OUTPUT);     // Initialize the BUILTIN_LED pin as an output
  Serial.begin(115200);
  setup_wifi();
  client.setServer(mqtt_server, 1883);
  client.setCallback(callback);
  
  // Setting up the IR (infrared emitter)
  irsend.begin();
#if ESP8266
  Serial.begin(115200, SERIAL_8N1, SERIAL_TX_ONLY);
#else  // ESP8266
  Serial.begin(115200, SERIAL_8N1);
#endif  // ESP8266

  dht.begin();
  sensor_t sensor;
  dht.temperature().getSensor(&sensor);
  dht.humidity().getSensor(&sensor);
  delayMS = 1500; //sensor.min_delay / 1000;
}

void loop() {

  if (!client.connected()) {
    reconnect();
  }
  client.loop();
  
  delay(delayMS);
  sensors_event_t event;
  dht.temperature().getEvent(&event);
  
  // Get temperature
  if (isnan(event.temperature)) {
    Serial.println(F("Error reading temperature!"));
  }
  else {
    if (lastTemperature != event.temperature){
      lastTemperature = event.temperature;
      dtostrf(event.temperature, 5, 2, temperature);
      strcpy(deviceInfoPayload, unitCode);
      strcat(deviceInfoPayload, "/t/");
      strcat(deviceInfoPayload, temperature);
      client.publish("DeviceInfo", deviceInfoPayload);
      Serial.println(deviceInfoPayload);
    }
  }
  
  // Get humidity
  dht.humidity().getEvent(&event);
  if (isnan(event.relative_humidity)) {
    Serial.println(F("Error reading humidity!"));
  }
  else {    
    if (lastHumidity != event.relative_humidity){
      lastHumidity = event.relative_humidity;
      dtostrf(event.relative_humidity, 5, 2, humidity);
      strcpy(deviceInfoPayload, unitCode);
      strcat(deviceInfoPayload, "/h/");
      strcat(deviceInfoPayload, humidity);
      client.publish("DeviceInfo", deviceInfoPayload);	  
      Serial.println(deviceInfoPayload);
    }	  
  }
}
