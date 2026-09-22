/**
 * The one place the backend's URL is configured. GameService reads this
 * instead of hard-coding "http://localhost:5000" itself, so pointing the app
 * at a different host (e.g. a deployed API) only ever means editing this file.
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
