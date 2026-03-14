<p align="center">
  <img src="https://github.com/harleykradovill/jfowui/blob/main/banners/providerlens.png?raw=true" width="300px">
</p>

## About

See which movies and TV shows in your libraries are available on other streaming services.
Utilizes the TMDB API, and identifies external streaming availability so administrators can easily decide what media is worth keeping locally.

Provider Lens is made for Jellyfin server owners who only like to keep exclusive media that is not available on other streaming services that they subscribe to.

## Configuration

TMDB API Key:

1. Create an account at [https://www.themoviedb.org/](https://www.themoviedb.org/).
2. Navigate to [https://www.themoviedb.org/settings/api](https://www.themoviedb.org/settings/api).
3. Your TMDB API key will be at the bottom of the page.

Streaming Region:

1. Navigate to [https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes](https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes).
2. Find your country, and make note of the **A-2** country code.
3. Enter this value in Provider Lens configuration.

Streaming Services:

Choose the streaming services you are subscribed to. Your streaming services here are matched to media items in Jellyfin.

Monitored Libraries:

Here you pick which libraries that you would like Provider Lens to check.

---

Data in Provider Lens will be generated when the scheduled task runs.

## Installation

Installation instructions are provided in my plugin repository.

[https://github.com/harleykradovill/jfowui](https://github.com/harleykradovill/jfowui)

## License

This project is licensed under the terms of the [GNU GPL v3.0](LICENSE).
