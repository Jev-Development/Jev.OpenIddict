import React, { Component, ReactElement } from 'react';
import { Container } from 'react-bootstrap';
import { Provider } from 'react-redux';
import { Navigate, NavigateFunction, Navigator, Route, Routes, useNavigate } from 'react-router-dom';
import './app.scss';
import { NavbarComponent } from './components/navbar/navbar.component';
import { RequireAnonymousGuard } from './navigation-guards/require-anonymous.guard';
import { RequireAuthenticationGuard } from './navigation-guards/require-authentication.guard';
import { LoginPage } from './pages/login/login.page';
import { NotFoundPage } from './pages/not-found/not-found.page';
import { ProfilePage } from './pages/profile/profile.page';
import { RegisterPage } from './pages/regsiter/register.page';
import { ApplicationStore } from './store';
import { AlertComponent } from './components/alert/alert.component';
import { ConsentPage } from './pages/consent/consent.page';

export function App(): ReactElement {
  const navigation: NavigateFunction = useNavigate();

  return (
    <Provider store={ApplicationStore}>
      <Container className="full-page p-0" fluid>
        <NavbarComponent></NavbarComponent>
        <div className="body container-fluid">
          <Routes>
            <Route path="/" element={<Navigate to="login" />} />
            <Route
              path="login"
              element={
                <RequireAnonymousGuard>
                  <LoginPage />
                </RequireAnonymousGuard>
              }
            />
            <Route
              path="register"
              element={
                <RequireAnonymousGuard>
                  <RegisterPage dispatch={ApplicationStore.dispatch} navigate={navigation} />
                </RequireAnonymousGuard>
              }
            />
            <Route
              path="profile"
              element={
                <RequireAuthenticationGuard>
                  <ProfilePage />
                </RequireAuthenticationGuard>
              }
            />
            <Route
              path="consent"
              element={
                <RequireAuthenticationGuard>
                  <ConsentPage />
                </RequireAuthenticationGuard>
              }
            />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
          <AlertComponent></AlertComponent>
        </div>
        <div className="footer">
          <p>
            Jev Dev
            <br />
            2022
          </p>
        </div>
      </Container>
    </Provider>
  );
}
