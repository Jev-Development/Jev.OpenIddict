import axios, { AxiosError, AxiosResponse } from 'axios';
import { Action } from 'redux';
import { ThunkDispatch } from 'redux-thunk';
import { ApplicationState } from '../states/application.state';

type UserStateActionType = 'CHECK_LOGGED_IN' | 'UPDATE_USER_STATE';

export interface UserStateAction extends Action<UserStateActionType> {
  data?: {
    loggedIn: boolean;
  };
}

export class UserActions {
  public static checkLoggedIn(): (
    dispatch: ThunkDispatch<ApplicationState, void, UserStateAction>,
    getState: () => ApplicationState
  ) => void {
    return (dispatch) => {
      axios
        .get<boolean>('api/account/logged-in')
        .then((response: AxiosResponse<boolean>) => {
          dispatch(UserActions.updateUserState(!!response?.data));
        })
        .catch((error: AxiosError) => {
          dispatch(UserActions.updateUserState(false));
        });
    };
  }

  public static updateUserState(loggedIn: boolean = false): UserStateAction {
    return {
      type: 'UPDATE_USER_STATE',
      data: {
        loggedIn: loggedIn
      }
    };
  }
}
