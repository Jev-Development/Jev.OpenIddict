import React from 'react';
import { Container, Nav, Navbar } from 'react-bootstrap';
import { useSelector } from 'react-redux';
import { Link, Location, useLocation } from 'react-router-dom';
import { LoggedInState } from '../../enums/logged-in-state.enum';
import { userStateSelector } from '../../selectors/user-state.selector';
import { UserState } from '../../states/user.state';

export function NavbarComponent(): React.ReactElement {
  const state: UserState = useSelector(userStateSelector);

  const location: Location = useLocation();

  const pathSegments: string[] = location.pathname.split('/');

  return (
    <Navbar bg="dark" variant="dark">
      <Container fluid>
        <Navbar.Brand href="#">Jev OpenIddict</Navbar.Brand>
        <Nav>
          {state.LoggedIn === LoggedInState.In && <Nav.Link href="/api/account/logout">Logout</Nav.Link>}
          {state.LoggedIn === LoggedInState.Out && (
            <Link className={pathSegments[1] === 'login' ? 'nav-link active' : 'nav-link'} to="/login">
              Login
            </Link>
          )}
          {state.LoggedIn === LoggedInState.Out && (
            <Link className={pathSegments[1] === 'register' ? 'nav-link active' : 'nav-link'} to="/register">
              Register
            </Link>
          )}
        </Nav>
      </Container>
    </Navbar>
  );
}
