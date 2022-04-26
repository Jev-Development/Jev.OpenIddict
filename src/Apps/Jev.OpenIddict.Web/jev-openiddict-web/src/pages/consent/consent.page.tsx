import React, { useState } from 'react';
import { Button, Card, Col, Form, Row, Spinner } from 'react-bootstrap';
import { useSearchParams } from 'react-router-dom';
import './consent.page.scss';
import { ApplicationApiService } from '../../services/application.services';
import { ApplicationViewDto } from '../../dto/application-view.dto';
import { LoadingComponent } from '../../components/loading/loading.component';

export function ConsentPage(): React.ReactElement {
  const [state, setState]: [string, (s: string) => void] = useState('');
  const [search, _]: readonly [URLSearchParams, any] = useSearchParams();

  const clientId: string = getClientId(search);
  const scopes: string[] = getScopes(search);

  const formValues: [string, string][] = [];

  search.forEach((val: string, key: string) => {
    formValues.push([key, val]);
  });

  ApplicationApiService.getByClientId(clientId).then((app: ApplicationViewDto) => {
    setState(app.name);
  });

  return !!state ? (
    <Row className="mt-2">
      <Col>
        <Card>
          <Card.Header>{state}</Card.Header>
          <Card.Body>
            <p>
              This app is requesting consent for the following scopes:
            </p>
            <ul>
                {scopes.map((scope: string, index: number) => (
                  <li key={`scopes_${index}`}>{scope}</li>
                ))}
              </ul>
            <Form action="/connect/authorize" method="post">
              <input type="hidden" name="accept" value="true" />
              {formValues.map(([key, val], index) => {
                return <input type="hidden" name={key} value={val} key={`accept_${index}`} />;
              })}
              <Button variant="outline-primary" className="m-2" type="submit">
                Accept
              </Button>
            </Form>
            <Form action="/connect/authorize" method="post">
              <input type="hidden" name="deny" value="true" />
              {formValues.map(([key, val], index) => {
                return <input type="hidden" name={key} value={val} key={`deny_${index}`} />;
              })}
              <Button variant="outline-danger" className="m-2" type="submit">
                Deny
              </Button>
            </Form>
          </Card.Body>
        </Card>
      </Col>
    </Row>
  ) : (
    <LoadingComponent />
  );
}

function getClientId(search: URLSearchParams): string {
  return search.get('client_id') ?? '';
}

function getScopes(search: URLSearchParams): string[] {
  return search.get('scope')?.split(' ') ?? [];
}
