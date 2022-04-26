import { Action } from 'redux';
import { AlertModel } from '../models/alert.model';

type AlertActionType = 'ADD' | 'REMOVE_AT_INDEX';

export interface AlertAction extends Action<AlertActionType> {
  data: {
    alertModel?: AlertModel;
    index?: number;
  };
}

export class AlertActions {
  public static addToQueue(alertModel: AlertModel): AlertAction {
    return {
      type: 'ADD',
      data: {
        alertModel: alertModel
      }
    };
  }

  public static removeAtIndex(index: number): AlertAction {
    return {
      type: 'REMOVE_AT_INDEX',
      data: {
        index: index
      }
    };
  }
}
