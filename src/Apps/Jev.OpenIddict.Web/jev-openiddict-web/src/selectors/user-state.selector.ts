import { ApplicationState } from '../states/application.state';
import { UserState } from '../states/user.state';

export function userStateSelector(state: ApplicationState): UserState {
  return state.userState;
}
