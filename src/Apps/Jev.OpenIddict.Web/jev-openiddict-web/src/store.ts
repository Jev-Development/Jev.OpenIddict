import { applyMiddleware, createStore, Store } from 'redux';
import logger from 'redux-logger';
import thunk from 'redux-thunk';
import { rootReducer } from './reducers/root.reducer';
import { ApplicationState } from './states/application.state';

export const ApplicationStore: Store<ApplicationState> = createStore(rootReducer(), applyMiddleware(thunk, logger));
