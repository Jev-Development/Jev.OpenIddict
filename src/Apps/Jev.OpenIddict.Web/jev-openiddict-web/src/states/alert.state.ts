import { AlertModel } from '../models/alert.model';

export interface AlertState {
  queue: AlertModel[];
}

export function getDefaultAlertState(): AlertState {
  return {
    queue: []
  };
}
