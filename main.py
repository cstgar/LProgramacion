import datetime as dt
import requests

BASE_URL = "https://api.openweathermap.org/data/2.5/weather?"
API_KEY = open('api_key', 'r').read()
# q representa los paramatros en la api
PARAMETERS = "&q="
COUNTRY = "MX"
CITY = " La Paz"
ID = "4000900"
ZIPCODE = "23085"
LENG = "es"

def kelvin_to_celsius_fahrenheit(kelvin):
    celsius = kelvin - 273.15
    fahrenheit = celsius * 1.8 + 32
    return celsius, fahrenheit

url = BASE_URL + "appid=" + API_KEY + PARAMETERS +CITY + "," + COUNTRY + "&lang=" + LENG
url_2 = BASE_URL + "appid=" + API_KEY + PARAMETERS +CITY + "," + COUNTRY + "&lang="
urlWithId = BASE_URL + "appid=" + API_KEY + PARAMETERS + "&id=" +ID
urlZip = BASE_URL + "appid=" + API_KEY + PARAMETERS + "&zip=" + ZIPCODE +"," + COUNTRY
response = requests.get(url).json()

# otra forma de hacer la peticion a la api de la misma ciudad solo con el id
response2 = requests.get(url_2).json()
#response3 = requests.get(urlZip).json()

#print(response)
#print(response2)
#print(response3)


temp_kelvin = response2["main"]["temp"]
temp_celsius, temp_fahrenheit = kelvin_to_celsius_fahrenheit(temp_kelvin)
#print(temp_celsius)
# print(temp_fahrenheit)
feels_like_kelvin = response2["main"]["feels_like"]
feels_like_celsius, feels_fah = kelvin_to_celsius_fahrenheit(feels_like_kelvin)
#print(feels_like_celsius)
humidity = response2["main"]["humidity"]
#print(humidity)
description = response2["weather"][0]["description"]
description_es = response["weather"][0]["description"]
#print(description)
wind_speed = response2["wind"]["speed"]
#print(wind_speed)

#se transforma porque la zona horaria que se da es de utc y se hacen los calculos para lo local
sunrise_timestamp_utc = response2["sys"]["sunrise"]
sunrise_utc = dt.datetime.fromtimestamp(sunrise_timestamp_utc, dt.UTC)
desplazamiento_segundos = response2["timezone"]
sunrise_local = sunrise_utc + dt.timedelta(seconds=desplazamiento_segundos)
#print(sunrise_local)

sunset_timestamp_utc = response2["sys"]["sunset"]
sunset_utc = dt.datetime.fromtimestamp(sunset_timestamp_utc, dt.UTC)
sunset_local = sunset_utc + dt.timedelta(seconds=desplazamiento_segundos)
#print(sunset_local)

#imprimiendo con formato
print("\n<<<<><><><><><><          ESPAÑOL           <><><><><><><><><><><><><")
print(f"Temperatura en {CITY}: {temp_celsius:.2f}°C o {temp_fahrenheit:.2f}°F")
print(f"Temperatura en {CITY} con sensación térmica de: {feels_like_celsius:.2f}°C o {feels_fah:.2f}°F")
print(f"Humedad: {humidity:.2f}%")
print(f"Vientos de: {wind_speed:.2f} m/h")
print(f"Condiciones Generales del Clima: {description_es}")
print(f"Amanece en {CITY} a las {sunrise_local.strftime('%H:%M:%S %d-%m-%Y')} hora local.")
print(f"Anochece en {CITY} a las {sunset_local.strftime('%H:%M:%S %d-%m-%Y')} hora local.")
print("\n")
print("<<<<><><><><><><><>><><><>INGLES<><><><><><><><><><><><><><><><><><><><")
print(f"Temperature in {CITY}: {temp_celsius:.2f}°C or {temp_fahrenheit:.2f}°F")
print(f"Temperature in {CITY} feels like: {feels_like_celsius:.2f}°C or {feels_fah:.2f}°F")
print(f"Humidity: {humidity:.2f}%")
print(f"Wind speed: {wind_speed:.2f} m/h")
print(f"General Weather Conditions: {description}")
print(f"Sun rises in {CITY} at {sunrise_local.strftime('%H:%M:%S %m-%d-%Y')} local time.")
print(f"Sun sets in {CITY} at {sunset_local.strftime('%H:%M:%S %m-%d-%Y')} local time.")

