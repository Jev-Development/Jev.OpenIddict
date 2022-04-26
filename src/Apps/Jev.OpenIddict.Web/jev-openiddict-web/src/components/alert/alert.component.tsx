import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { alertStateSelector } from '../../selectors/alert-state.selector';
import { AlertState } from '../../states/alert.state';
import { AlertModel } from '../../models/alert.model';
import { Alert } from 'react-bootstrap';
import './alert.component.scss';
import { AnyAction, Dispatch } from 'redux';
import { AlertActions } from '../../actions/alert-state.actions';

export function AlertComponent(): React.ReactElement {
  const alertState: AlertState = useSelector(alertStateSelector);
  const dispatch: Dispatch<AnyAction> = useDispatch();

  return (
    <div className="alert-container p-3">
      {alertState.queue.map((item: AlertModel, index: number) => {
        return (
          <Alert variant={item.type} key={index} dismissible onClose={() => dispatch(AlertActions.removeAtIndex(index))}>
            {!!item.header && <Alert.Heading>{item.header}</Alert.Heading>}
            {item.message}
          </Alert>
        );
      })}
    </div>
  );
}
