import { AlertState, getDefaultAlertState } from '../states/alert.state';
import { AlertAction } from '../actions/alert-state.actions';
import { AlertModel } from '../models/alert.model';

export function AlertStateReducer(state: AlertState = getDefaultAlertState(), action: AlertAction): AlertState {
  switch (action.type) {
    case 'ADD':
      if (!action.data.alertModel) throw 'Alert needs to be defined when using the ADD action.';
      return {
        queue: [...state.queue, action.data.alertModel]
      };
    case 'REMOVE_AT_INDEX':
      const resultArr: AlertModel[] = [...state.queue];
      if (action.data.index === undefined || action.data.index === null)
        throw 'Alert index needs to be defined for the REMOVE_AT_INDEX action.';
      resultArr.splice(action.data.index, 1);
      return {
        queue: resultArr
      };
    default:
      return state;
  }
}
