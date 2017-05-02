# README #

A Tv-show tracker that uses TheTVDB.com API v2. Search and add series to your trackinglist and get notified when new episodes are aired for your favourite series.

Right now only "valid" episodes are being processed. There are special-episodes and episodes without a correctly formatted air-date that will be ignored from processing.

Feel free to contribute!

### How to run ###

To run this you need

1. Your own api-keys at TheTVDB.com
2. Run CLI to make a skeleton config in Data-folder
2. Modify Data/EpisodeTracker.json with your api credentials and endpoint to the api

Example of ApiCredentials-element:

```
"ApiCredentials": {
    "$type": "EpisodeTracker.Storage.ApiCredentials, EpisodeTracker",
    "ApiUrl": "https://api.thetvdb.com/",
    "ApiKey": "",
    "ApiUser": "",
    "ApiUserkey": ""
  }
```

### Examples ###

A small example-application as a commandline-project is provided with a json-file storage.

The cli could be wrapped in a windows-service (or just the windows scheduler perhaps) to call the update-method frequently without having to manually do much except mark episodes as seen and track new series.

(At this point, if any starting arguments are given to the CLI executable, only the update-method is run without any output.)
