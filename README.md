# StravaExample
Example for authenticate, download and upload activities between Strava and Xamarin.Forms app.

<img src="https://github.com/marciosouzajunior/StravaExample/blob/master/StravaExample/StravaExample/screenshot.png" width="250">

## Project structure
- **Models**: Classes that represent Strava and app entities.
- **Services**:
  - *StravaService*: Main class to connect, disconnect and sync.
  - *Database*: Uses SQLite to persist activities.
  - *Dialog*: Abstracts alert dialog methods.
  - *HttpHandler*: Abstracts HttpClient methods (to allow unit testing).
  - *Properties*: Used to store application-scope properties.
  - *DateTimeUtil*: DateTime conversion utility class.
- **MainPage**: Basic UI interface to connect, disconnect, add activity and sync buttons.

## How to use
1. Configure StravaService class with your clientId, clientSecret and redirectUri. Connect and follow authorization process.
2. Tap new activity button to create a mock activity and then sync to send to Strava.
3. Create an activity on Strava and tap sync to download.
