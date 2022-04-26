import React, { Dispatch } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Navigate } from 'react-router-dom';
import { AnyAction } from 'redux';
import { UserActions } from '../actions/user-state.actions';
import { LoadingComponent } from '../components/loading/loading.component';
import { LoggedInState } from '../enums/logged-in-state.enum';
import { userStateSelector } from '../selectors/user-state.selector';
import { UserState } from '../states/user.state';

export function RequireAuthenticationGuard(props: { children: React.ReactElement }): React.ReactElement {
  const state: UserState = useSelector(userStateSelector);
  const dispatch: Dispatch<AnyAction> = useDispatch();

  switch (state.LoggedIn) {
    case LoggedInState.NotChecked:
      dispatch(UserActions.checkLoggedIn() as any);
      return <LoadingComponent />;
    case LoggedInState.In:
      return props.children;
    case LoggedInState.Out:
      return <Navigate to="/login" />;
    default:
      return <Navigate to="/error" />;
  }
}
