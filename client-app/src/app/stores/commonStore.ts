import { makeAutoObservable, reaction } from "mobx";
import { ServerError } from "../models/serverError";

const jwtTokenKey = 'jwt';

export default class CommonStore {
    error: ServerError | null = null;
    token: string | null | undefined = localStorage.getItem(jwtTokenKey);
    appLoaded = false;

    constructor() {
        makeAutoObservable(this);

        reaction(
            () => this.token,
            token => {
                if (token) {
                    localStorage.setItem(jwtTokenKey, token);
                }
                else {
                    localStorage.removeItem(jwtTokenKey);
                }
            }
        )
    }

    setServerError(error: ServerError) {
        this.error = error;
    }

    setToken = (token: string | null) => {
        this.token = token;
    }

    setAppLoaded = () => {
        this.appLoaded = true;
    }
}