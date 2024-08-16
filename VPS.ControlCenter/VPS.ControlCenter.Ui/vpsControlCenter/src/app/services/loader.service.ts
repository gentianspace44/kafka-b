import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoaderService {

  isLoading: boolean = false;

  constructor() {}

  show() {
    this.isLoading = true;
  }

  hide() {
    this.isLoading = false;
  }
}
