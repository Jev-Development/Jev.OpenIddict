import { Formik, FormikErrors, FormikHelpers, FormikValues } from 'formik';
import * as joi from 'joi';
import React, { Component, Dispatch, ReactElement } from 'react';
import { Button, Card, Col, Form, Row } from 'react-bootstrap';
import { AnyAction } from 'redux';
import { GetDefaultRegisterModel, RegisterModel } from '../../models/register.model';
import { OpenIddictService } from '../../services/jev-openiddict.service';
import './register.page.scss';
import { NavigateFunction } from 'react-router-dom';

export class RegisterPage extends Component<{ dispatch: Dispatch<AnyAction>; navigate: NavigateFunction }> {
  public onSubmit: (values: RegisterModel, helpers: FormikHelpers<RegisterModel>) => void = (values, helpers) => {
    helpers.setStatus('submitting');

    OpenIddictService.register({
      email: values.email,
      password: values.password
    })
      .then((_) => {
        this.props.navigate('/login');
      })
      .catch(() => {
        helpers.setStatus(null);
      });
  };

  private _hasBeenTouched: boolean = false;

  constructor(props: { dispatch: Dispatch<AnyAction>; navigate: NavigateFunction }) {
    super(props);
  }

  public validate: (values: RegisterModel) => FormikErrors<FormikValues> = (values: RegisterModel) => {
    this._hasBeenTouched = true;
    const schema: joi.ObjectSchema<RegisterModel> = RegisterPage.getValidationSchema();

    const validationResult: joi.ValidationResult<RegisterModel> = schema.validate(values, {
      abortEarly: false
    });

    const result: FormikErrors<FormikValues> = {};

    validationResult.error?.details.forEach((item: joi.ValidationErrorItem) => {
      result[item.path[0] as string] = item.message;
    });

    return result;
  };

  public render(): ReactElement {
    return (
      <Row className="justify-content-center">
        <Col className="p-3 register-box" md="auto">
          <Card>
            <Card.Body>
              <h3>Register an account</h3>
              <Formik initialValues={GetDefaultRegisterModel()} validate={this.validate} onSubmit={this.onSubmit}>
                {({ handleSubmit, handleChange, handleBlur, values, touched, isValid, errors, status }) => (
                  <Form noValidate onSubmit={handleSubmit}>
                    <Row className="mb-3">
                      <Col>
                        <Form.Group controlId="email">
                          <Form.Label>Email</Form.Label>
                          <Form.Control
                            type="text"
                            name="email"
                            value={values.email}
                            onBlur={handleBlur}
                            onChange={handleChange}
                            isInvalid={!!touched.email && !!errors?.email}
                            disabled={status === 'submitting'}
                          />
                          <Form.Control.Feedback type="invalid">{errors.email}</Form.Control.Feedback>
                        </Form.Group>
                      </Col>
                    </Row>
                    <Row className="mb-3">
                      <Col>
                        <Form.Group controlId="password">
                          <Form.Label>Password</Form.Label>
                          <Form.Control
                            type="password"
                            name="password"
                            value={values.password}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            isInvalid={!!touched?.password && !!errors?.password}
                            disabled={status === 'submitting'}
                          />
                          <Form.Control.Feedback type="invalid">{errors?.password}</Form.Control.Feedback>
                        </Form.Group>
                      </Col>
                    </Row>
                    <Row className="mb-3">
                      <Col>
                        <Form.Group controlId="confirmPassword">
                          <Form.Label>Password</Form.Label>
                          <Form.Control
                            type="password"
                            name="confirmPassword"
                            value={values.confirmPassword}
                            onChange={handleChange}
                            onBlur={handleBlur}
                            isInvalid={!!touched?.confirmPassword && !!errors?.confirmPassword}
                            disabled={status === 'submitting'}
                          />
                          <Form.Control.Feedback type="invalid">{errors?.confirmPassword}</Form.Control.Feedback>
                        </Form.Group>
                      </Col>
                    </Row>
                    <Row className="mb-3">
                      <Col className="mx-auto" md="auto">
                        <Button
                          type="submit"
                          variant="outline-primary"
                          disabled={!isValid || !this._hasBeenTouched || status === 'submitting'}
                        >
                          Register
                        </Button>
                      </Col>
                    </Row>
                  </Form>
                )}
              </Formik>
            </Card.Body>
          </Card>
        </Col>
      </Row>
    );
  }

  private static getValidationSchema(): joi.ObjectSchema<RegisterModel> {
    return joi.object<RegisterModel>({
      email: joi
        .string()
        .email({
          tlds: {
            allow: false
          }
        })
        .required(),
      password: joi.string().min(6).required(),
      confirmPassword: joi.string().min(6).equal(joi.ref('password')).required()
    });
  }
}
