import { combineReducers, Reducer } from 'redux';
import { ApplicationState } from '../states/application.state';
import { UserStateReducer } from './user-state.reducer';
import { AlertStateReducer } from './alert-state.reducer';

export function rootReducer(): Reducer<ApplicationState> {
  return combineReducers({
    userState: UserStateReducer,
    alertState: AlertStateReducer
  });
}
