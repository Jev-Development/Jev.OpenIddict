import { UserState } from './user.state';
import { AlertState } from './alert.state';

export interface ApplicationState {
  userState: UserState;
  alertState: AlertState;
}
