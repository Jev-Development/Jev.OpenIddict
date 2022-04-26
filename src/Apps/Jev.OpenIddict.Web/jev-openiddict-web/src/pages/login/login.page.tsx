import React, { Component, FormEvent, ReactElement } from 'react';
import { Button, Col, Form, Row } from 'react-bootstrap';
import './login.page.scss';
import { useSearchParams } from 'react-router-dom';

export function LoginPage(): React.ReactElement {
  const [search, _]: readonly [URLSearchParams, any] = useSearchParams();

  search.toString();

  return (
    <Row className="login-row">
      <Col className="login-box m-auto p-3" md="auto">
        <Form action={`api/account/login?${search.toString()}`} method="post">
          <Form.Group className="mb-3">
            <Form.Label>Email Address</Form.Label>
            <Form.Control name="username" id="username" type="email" placeholder="Enter Email" required></Form.Control>
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Password</Form.Label>
            <Form.Control name="password" id="password" type="password" required placeholder="Password" />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Check id="rememberMe" name="rememberMe" type="checkbox" label="Remember Me" />
          </Form.Group>
          <Button variant="outline-primary" type="submit">
            Submit
          </Button>
        </Form>
      </Col>
    </Row>
  );
}
