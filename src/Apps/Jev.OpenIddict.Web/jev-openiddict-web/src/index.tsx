import React from 'react';
import { ThemeProvider } from 'react-bootstrap';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import { App } from './app';
import './index.scss';

ReactDOM.render(
  <ThemeProvider breakpoints={['xxxl', 'xxl', 'xl', 'lg', 'md', 'sm', 'xs', 'xxs']}>
    <BrowserRouter basename={base()}>
      <App />
    </BrowserRouter>
  </ThemeProvider>,
  document.getElementById('app-root')
);

function base(): string {
  return document.querySelector('base')?.attributes.getNamedItem('href')?.value ?? '/';
}
