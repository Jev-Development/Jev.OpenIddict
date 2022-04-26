import { GetDefaultUserState, UserState } from '../states/user.state';
import { UserStateAction } from '../actions/user-state.actions';
import { LoggedInState } from '../enums/logged-in-state.enum';

export function UserStateReducer(state: UserState = GetDefaultUserState(), action: UserStateAction): UserState {
  switch (action.type) {
    case 'UPDATE_USER_STATE':
      return {
        ...state,
        LoggedIn: !!action?.data?.loggedIn ? LoggedInState.In : LoggedInState.Out
      };
    case 'CHECK_LOGGED_IN':
    default:
      return state;
  }
}
