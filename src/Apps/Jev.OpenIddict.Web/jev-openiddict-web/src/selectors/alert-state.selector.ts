import { ApplicationState } from '../states/application.state';
import { AlertState } from '../states/alert.state';

export function alertStateSelector(applicationState: ApplicationState): AlertState {
  return applicationState.alertState;
}
