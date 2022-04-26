import React from 'react';
import { Spinner } from 'react-bootstrap';
import './loading.scss';

export class LoadingComponent extends React.Component {
  public render(): React.ReactElement {
    return (
      <div id="loading">
        <Spinner animation="border" className='my-auto'/>
      </div>
    );
  }
}
