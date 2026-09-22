import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';

/**
 * Root providers for the standalone bootstrap in main.ts.
 * provideHttpClient() is the one piece of infrastructure GameService needs -
 * registered exactly once, here, rather than in every component that happens
 * to use it.
 */
export const appConfig: ApplicationConfig = {
  providers: [provideHttpClient()]
};
