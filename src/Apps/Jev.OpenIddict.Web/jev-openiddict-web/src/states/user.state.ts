import { LoggedInState } from '../enums/logged-in-state.enum';
export interface UserState {
  LoggedIn: LoggedInState;
}

export function GetDefaultUserState(): UserState {
  return { LoggedIn: LoggedInState.NotChecked };
}
